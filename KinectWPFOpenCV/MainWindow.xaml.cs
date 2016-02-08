using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using OSC.NET;
using TSPS;
using System.Net;
using System.Collections;
using GlobalKeyboardHook;
using System.Windows.Interop;
using System.Reflection;
using System.Diagnostics;
using raw_streams.cs;

namespace KinectWPFOpenCV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PXCMSession session;
        private List<PXCMDevice> devices = new List<PXCMDevice>();
        //private Dictionary<string, int> devices_iuid = new Dictionary<string, int>();
        private List<PXCMCapture.VideoStream.ProfileInfo> colorProfiles = new List<PXCMCapture.VideoStream.ProfileInfo>();
        private List<PXCMCapture.VideoStream.ProfileInfo> depthProfiles = new List<PXCMCapture.VideoStream.ProfileInfo>();

        BitmapSource depthBitmap;
        byte[] depthPixels;
        int width;
        BitmapSource colorBitmap;

        Image<Bgr, Byte> background;
        Image<Bgr, Byte> latestDepth;

        const int BackgroundValidationFrameCount = 60;
        const int BackgroundOverlayFrameCount = 2;
        const double BackgroundOverlayFactor = 0.1;
        int backgroundValidation = BackgroundValidationFrameCount;

        int blobCount = 0;
        int frameCounter = 0;
        int bgFrameCounter = 0;
        double deltaTime = 0;
        DateTime lastFrame;

        private volatile bool stop = false;

        //Osc
        OSCTransmitter udpWriter;

        //TSPS
        int _pid = 0;
        List<int> peopleEntered = new List<int>(); // Collection of pids
        Dictionary<int, Person> peopleLastFrame = new Dictionary<int, Person>(); // History of people in last frame, key is pid

        //Velocity calculation
        int historyFrameCount = 0;
        Dictionary<int, PersonHistory> peopleHistory = new Dictionary<int, PersonHistory>(); // History of people in 10 frames before, key is pid

        //Global hotkey
        KeyboardHandler backgroundCaptureKey;
        KeyboardHandler exitKey;

        Process proc;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.MouseDown += MainWindow_MouseDown;

        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FindSensor();
            backgroundCaptureKey = new KeyboardHandler(this, Key.F5);
            backgroundCaptureKey.keyboardEventHandler += AutoBackgroundCapture;
            exitKey = new KeyboardHandler(this, Key.Escape);
            exitKey.keyboardEventHandler += () =>
            {
                Application.Current.Shutdown();
            };
            this.WindowState = System.Windows.WindowState.Minimized;
            string autoLaunchPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "autoLaunch.lnk");
            if (File.Exists(autoLaunchPath))
            {
                proc = new Process();
                proc.StartInfo.FileName = autoLaunchPath;
                proc.Start();
            }
        }

        private void FindSensor()
        {
            this.session = null;
            pxcmStatus sts = PXCMSession.CreateInstance(out session);
            if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                txtInfo.Text = "Connected";
                sensor_Initialize();
            }
            else
            {
                txtInfo.Text = "Disconnected";
                sensor_NotReady();
            }
        }

        delegate void StatusChanged(object sender, string e);
        public void sensor_StatusChanged(object sender, string e)
        {
            StatusChanged d = new StatusChanged(OnStatusChanged);
            Dispatcher.Invoke(d, new Object[] { sender, e });
        }

        private void OnStatusChanged(object sender, string e)
        {
            this.txtInfo.Text = e;
        }

        private void sensor_Initialize()
        {
            if (null != this.session)
            {
                PopulateDeviceMenu();
                System.Threading.Thread thread = new System.Threading.Thread(DoRendering);
                thread.Start();
                System.Threading.Thread.Sleep(5);
                //this.colorImg.Source = this.colorBitmap;
            }
            backgroundValidation = BackgroundValidationFrameCount;
            this.outputViewbox.Visibility = System.Windows.Visibility.Visible;
            this.txtError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void PopulateDeviceMenu()
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR;
            desc.subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_VIDEO_CAPTURE;

            devices.Clear();
            //DeviceMenu.DropDownItems.Clear();
            for (uint i = 0; ; i++)
            {
                PXCMSession.ImplDesc desc1;
                if (session.QueryImpl(ref desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                PXCMCapture capture;
                if (session.CreateImpl<PXCMCapture>(ref desc1, PXCMCapture.CUID, out capture) < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;
                for (uint j = 0; ; j++)
                {
                    PXCMCapture.DeviceInfo dinfo;
                    if (capture.QueryDevice(j, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    devices.Add(new PXCMDevice(dinfo, desc1.iuid));
                    //ToolStripMenuItem sm1 = new ToolStripMenuItem(dinfo.name.get(), null, new EventHandler(Device_Item_Click));
                    //devices[sm1] = dinfo;
                    //devices_iuid[sm1] = desc1.iuid;
                    //DeviceMenu.DropDownItems.Add(sm1);
                }
                capture.Dispose();
            }
            /*
            if (DeviceMenu.DropDownItems.Count > 0)
                (DeviceMenu.DropDownItems[0] as ToolStripMenuItem).Checked = true;
                */
            if (devices.Count > 0)
                PopulateColorDepthMenus(devices[0]);
        }

        private void PopulateColorDepthMenus(PXCMDevice selectedDevice)
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR;
            desc.subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_VIDEO_CAPTURE;
            desc.iuid = selectedDevice.iuid;
            desc.cuids[0] = PXCMCapture.CUID;

            colorProfiles.Clear();
            depthProfiles.Clear();
            //ColorMenu.DropDownItems.Clear();
            //DepthMenu.DropDownItems.Clear();
            PXCMCapture capture;
            if (session.CreateImpl<PXCMCapture>(ref desc, PXCMCapture.CUID, out capture) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMCapture.Device device;
                if (capture.CreateDevice(selectedDevice.device.didx, out device) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    bool cpopulated = false, dpopulated = false;
                    for (uint s = 0; ; s++)
                    {
                        PXCMCapture.Device.StreamInfo sinfo;
                        if (device.QueryStream(s, out sinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                        if (sinfo.cuid != PXCMCapture.VideoStream.CUID) continue;

                        if (((int)sinfo.imageType & (int)PXCMImage.ImageType.IMAGE_TYPE_COLOR) != 0 && !cpopulated)
                        {
                            PXCMCapture.VideoStream stream;
                            if (device.CreateStream<PXCMCapture.VideoStream>(s, PXCMCapture.VideoStream.CUID, out stream) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                            {
                                for (uint p = 0; ; p++)
                                {
                                    PXCMCapture.VideoStream.ProfileInfo pinfo;
                                    if (stream.QueryProfile(p, out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                                    //ToolStripMenuItem sm1 = new ToolStripMenuItem(ProfileToString(pinfo), null, new EventHandler(Color_Item_Click));
                                    colorProfiles.Add(pinfo);
                                    //ColorMenu.DropDownItems.Add(sm1);
                                    cpopulated = true;
                                }
                                stream.Dispose();
                            }
                        }
                        if (((int)sinfo.imageType & (int)PXCMImage.ImageType.IMAGE_TYPE_DEPTH) != 0 && !dpopulated)
                        {
                            PXCMCapture.VideoStream stream;
                            if (device.CreateStream<PXCMCapture.VideoStream>(s, PXCMCapture.VideoStream.CUID, out stream) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                            {
                                for (uint p = 0; ; p++)
                                {
                                    PXCMCapture.VideoStream.ProfileInfo pinfo;
                                    if (stream.QueryProfile(p, out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                                    //ToolStripMenuItem sm1 = new ToolStripMenuItem(ProfileToString(pinfo), null, new EventHandler(Depth_Item_Click));
                                    depthProfiles.Add(pinfo);
                                    //DepthMenu.DropDownItems.Add(sm1);
                                    dpopulated = true;
                                }
                                stream.Dispose();
                            }
                        }
                    }
                    device.Dispose();
                }
                capture.Dispose();
            }
            //ColorNone = new ToolStripMenuItem("None", null, new EventHandler(Color_Item_Click));
            //profiles[ColorNone] = new PXCMCapture.VideoStream.ProfileInfo();
            //ColorMenu.DropDownItems.Add(ColorNone);
            //DepthNone = new ToolStripMenuItem("None", null, new EventHandler(Depth_Item_Click));
            //profiles[DepthNone] = new PXCMCapture.VideoStream.ProfileInfo();
            //DepthMenu.DropDownItems.Add(DepthNone);
            //(ColorMenu.DropDownItems[0] as ToolStripMenuItem).Checked = true;
            //(DepthMenu.DropDownItems[0] as ToolStripMenuItem).Checked = true;

            //CheckSelection();
        }

        private void DoRendering()
        {
            RenderStreams rs = new RenderStreams();
            rs.RunColorDepthAsync(this.devices[0], this.colorProfiles[0], this.depthProfiles[0], this);
        }

        private string ProfileToString(PXCMCapture.VideoStream.ProfileInfo pinfo)
        {
            string line = pinfo.imageInfo.format.ToString().Substring(13) + " " + pinfo.imageInfo.width + "x" + pinfo.imageInfo.height + " ";
            if (pinfo.frameRateMin.denominator != 0 && pinfo.frameRateMax.denominator != 0)
            {
                line += (float)pinfo.frameRateMin.numerator / pinfo.frameRateMin.denominator + "-" +
                      (float)pinfo.frameRateMax.numerator / pinfo.frameRateMax.denominator;
            }
            else
            {
                PXCMRatioU32 fps = (pinfo.frameRateMin.denominator != 0) ? pinfo.frameRateMin : pinfo.frameRateMax;
                line += (float)fps.numerator / fps.denominator;
            }
            return line;
        }

        public bool GetStopState()
        {
            return stop;
        }

        public bool GetDepthRawState()
        {
            return false;
        }

        public bool GetColorState()
        {
            return false;
        }

        public bool GetDepthState()
        {
            return true;
        }

        delegate void BitmapReadyCallback(int index, int width, int height, byte[] pixels);
        public void SetBitmap(int index, int width, int height, byte[] pixels)
        {
            BitmapReadyCallback d = new BitmapReadyCallback(OnBitmapReady);
            Dispatcher.Invoke(d, new Object[] { index, width, height, pixels });
        }

        private void OnBitmapReady(int index, int width, int height, byte[] pixels)
        {
            switch (index)
            {
                case 0:
                    this.depthBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
                    this.depthPixels = pixels;
                    this.width = width;
                    blobCount = 0;
                    BlobDetection();
                    if (lastFrame != null)
                    {
                        frameCounter++;
                        deltaTime += (DateTime.Now - lastFrame).TotalSeconds;
                        if (deltaTime >= 1)
                        {
                            txtFPS.Text = frameCounter.ToString() + " FPS";
                            deltaTime = 0;
                            frameCounter = 0;
                        }
                    }
                    lastFrame = DateTime.Now;
                    break;
                case 1:
                    colorBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
                    this.colorImg.Source = this.colorBitmap;
                    break;
                default:
                    break;
            }
        }

        private void sensor_NotReady()
        {
            this.outputViewbox.Visibility = System.Windows.Visibility.Collapsed;
            this.txtError.Visibility = System.Windows.Visibility.Visible;
        }

        void BlobDetection()
        {
            Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(this.depthBitmap.ToBitmap());

            if (chkFlipH.IsChecked == true)
                openCVImg = openCVImg.Flip(FLIP.HORIZONTAL);

            if (chkFlipV.IsChecked == true)
                openCVImg = openCVImg.Flip(FLIP.VERTICAL);

            if (latestDepth != null)
            {
                latestDepth.Dispose();
            }
            latestDepth = openCVImg.Copy();

            if (background != null)
            {
                openCVImg -= background;
                bgFrameCounter++;
                if (bgFrameCounter >= BackgroundOverlayFrameCount)
                {
                    //background = background.AddWeighted(latestDepth, 1.0 - BackgroundOverlayFactor, BackgroundOverlayFactor, 0.0);
                    bgFrameCounter = 0;
                }
            }

            //openCVImg._GammaCorrect(sliderGamma.Value);
            //openCVImg = openCVImg.ThresholdToZero(new Bgr(255 - sliderThreshold.Value, 255 - sliderThreshold.Value, 255 - sliderThreshold.Value));
            openCVImg = openCVImg.ThresholdBinary(new Bgr(sliderThreshold.Value, sliderThreshold.Value, sliderThreshold.Value), new Bgr(255, 255, 255));

            Image<Bgr, Byte> trackImg = new Image<Bgr, byte>(this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight);
            Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();

            using (MemStorage stor = new MemStorage())
            {
                //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                Contour<System.Drawing.Point> contours = gray_image.FindContours(
                 Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                 Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                 stor);

                Dictionary<int, Person> people = new Dictionary<int, Person>(); // blob of current frame, key is oid

                double lineWidthFactor = Math.Sqrt(trackImg.Width * trackImg.Height / (sliderMaxSize.Value * sliderMaxSize.Value));

                for (int i = 0; contours != null; contours = contours.HNext)
                {
                    i++;

                    if ((contours.Area > Math.Pow(sliderMinSize.Value, 2)) && (contours.Area < Math.Pow(sliderMaxSize.Value, 2)))
                    {
                        double centroidX = contours.BoundingRectangle.Location.X + contours.BoundingRectangle.Width / 2;
                        double centroidY = contours.BoundingRectangle.Location.Y + contours.BoundingRectangle.Height / 2;

                        //Contours
                        trackImg.Draw(contours, new Bgr(System.Drawing.Color.WhiteSmoke), (int)Math.Ceiling(lineWidthFactor));
                        //Centroid
                        trackImg.Draw(new Cross2DF(new System.Drawing.PointF((float)centroidX, (float)centroidY), (int)Math.Ceiling(10 * lineWidthFactor), (int)Math.Ceiling(10 * lineWidthFactor)), new Bgr(System.Drawing.Color.Yellow), (int)Math.Ceiling(2 * lineWidthFactor));
                        //BoundingRectangle
                        trackImg.Draw(contours.BoundingRectangle, new Bgr(System.Drawing.Color.Yellow), (int)Math.Ceiling(2 * lineWidthFactor));
                        //BoundingRectangle Origin
                        trackImg.Draw(new Cross2DF(new System.Drawing.PointF((float)contours.BoundingRectangle.Location.X, (float)contours.BoundingRectangle.Location.Y), (int)Math.Ceiling(10 * lineWidthFactor), (int)Math.Ceiling(10 * lineWidthFactor)), new Bgr(System.Drawing.Color.Green), (int)Math.Ceiling(2 * lineWidthFactor));
                        //MinAreaRect
                        //MCvBox2D box = contours.GetMinAreaRect();
                        //trackImg.Draw(box, new Bgr(System.Drawing.Color.Yellow), 2);
                        //Create the font
                        MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, lineWidthFactor, lineWidthFactor);
                        trackImg.Draw("oid: " + blobCount.ToString(), ref f, new System.Drawing.Point((int)centroidX + 10, (int)centroidY + 15), new Bgr(System.Drawing.Color.Yellow));

                        people.Add(blobCount, new Person());
                        people[blobCount].oid = blobCount;
                        people[blobCount].age = 0;
                        people[blobCount].boundingRectSizeWidth = (float)((double)contours.BoundingRectangle.Width / (double)gray_image.Width);
                        people[blobCount].boundingRectSizeHeight = (float)((double)contours.BoundingRectangle.Height / (double)gray_image.Height);
                        people[blobCount].boundingRectOriginX = (float)((double)contours.BoundingRectangle.Location.X / (double)gray_image.Width);
                        people[blobCount].boundingRectOriginY = (float)((double)contours.BoundingRectangle.Location.Y / (double)gray_image.Height);
                        people[blobCount].centroidX = (float)(centroidX / (double)gray_image.Width);
                        people[blobCount].centroidY = (float)(centroidY / (double)gray_image.Height);
                        people[blobCount].depth = (float)(this.depthPixels[(int)centroidX + (int)centroidY * this.width]);
                        people[blobCount].velocityX = 0;
                        people[blobCount].velocityY = 0;

                        blobCount++;
                    }
                }

                List<int> peopleOnStage = new List<int>();

                List<int> pidRemaining = new List<int>();
                foreach (int personId in peopleEntered)
                    pidRemaining.Add(personId);

                List<int> oidRemaining = new List<int>();
                for (int i = 0; i < blobCount; i++)
                    oidRemaining.Add(i);

                while (pidRemaining.Count > 0 && oidRemaining.Count > 0)
                {

                    double distance = -1;
                    int pid = -1;
                    int oid = -1;

                    // Find the closest pair
                    foreach (int i in oidRemaining)
                    {
                        Vector centroid = new Vector(people[i].centroidX, people[i].centroidY);
                        foreach (int personId in pidRemaining)
                        {
                            Vector centroidLastFrame = new Vector(peopleLastFrame[personId].centroidX, peopleLastFrame[personId].centroidY);
                            if (distance == -1 || distance > (centroid - centroidLastFrame).Length)
                            {
                                distance = (centroid - centroidLastFrame).Length;
                                oid = i;
                                pid = personId;
                            }
                        }
                    }

                    if (pid != -1 && oid != -1)
                    {
                        people[oid].id = pid;
                        if (peopleLastFrame[pid].age == int.MaxValue - 1) // Make sure that age does not overflow int, although it should never happen in real life.
                            peopleLastFrame[pid].age = 0;
                        people[oid].age = peopleLastFrame[pid].age + 1;
                        oidRemaining.Remove(oid);
                        pidRemaining.Remove(pid);

                        //Create the font
                        MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, lineWidthFactor, lineWidthFactor);
                        //Display pid and age
                        trackImg.Draw("pid: " + people[oid].id.ToString(), ref f, new System.Drawing.Point((int)((double)people[oid].centroidX * (double)gray_image.Width) + 10, (int)((double)people[oid].centroidY * (double)gray_image.Height) + 5), new Bgr(System.Drawing.Color.Yellow));
                        trackImg.Draw("age: " + people[oid].age.ToString(), ref f, new System.Drawing.Point((int)((double)people[oid].centroidX * (double)gray_image.Width) + 10, (int)((double)people[oid].centroidY * (double)gray_image.Height) + 30), new Bgr(System.Drawing.Color.Yellow));

                        if (historyFrameCount == 0)
                        {
                            if (peopleHistory.ContainsKey(pid))
                            {
                                people[oid].velocityX = (float)Math.Abs((people[oid].centroidX - peopleHistory[pid].centroidX) / (DateTime.Now - peopleHistory[pid].timeStamp).TotalSeconds * (double)gray_image.Width);
                                people[oid].velocityY = (float)Math.Abs((people[oid].centroidY - peopleHistory[pid].centroidY) / (DateTime.Now - peopleHistory[pid].timeStamp).TotalSeconds * (double)gray_image.Height);
                            }
                            peopleHistory[pid] = new PersonHistory(people[oid]);
                        }

                        PersonUpdate(people[oid]);
                        peopleLastFrame[pid] = people[oid];
                        peopleOnStage.Add(pid);
                        people.Remove(oid);
                    }
                }

                foreach (int i in oidRemaining)
                {
                    people[i].id = ++_pid;
                    people[i].age = 1;

                    //Create the font
                    MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, lineWidthFactor, lineWidthFactor);
                    //Display pid and age
                    trackImg.Draw("pid: " + people[i].id.ToString(), ref f, new System.Drawing.Point((int)((double)people[i].centroidX * (double)gray_image.Width) + 10, (int)((double)people[i].centroidY * (double)gray_image.Height) + 5), new Bgr(System.Drawing.Color.Yellow));
                    trackImg.Draw("age: " + people[i].age.ToString(), ref f, new System.Drawing.Point((int)((double)people[i].centroidX * (double)gray_image.Width) + 10, (int)((double)people[i].centroidY * (double)gray_image.Height) + 30), new Bgr(System.Drawing.Color.Yellow));

                    if (historyFrameCount == 0)
                    {
                        if (peopleHistory.ContainsKey(_pid))
                        {
                            people[i].velocityX = (float)Math.Abs((people[i].centroidX - peopleHistory[_pid].centroidX) / (DateTime.Now - peopleHistory[_pid].timeStamp).TotalSeconds * (double)gray_image.Width);
                            people[i].velocityY = (float)Math.Abs((people[i].centroidY - peopleHistory[_pid].centroidY) / (DateTime.Now - peopleHistory[_pid].timeStamp).TotalSeconds * (double)gray_image.Height);
                        }
                        peopleHistory[_pid] = new PersonHistory(people[i]);
                    }

                    PersonEnter(people[i]);
                    peopleLastFrame[_pid] = people[i];
                    peopleEntered.Add(_pid);
                    peopleOnStage.Add(_pid);
                }

                int[] peopleIds = peopleEntered.ToArray();
                foreach (int personId in peopleIds)
                {
                    if (!peopleOnStage.Contains(personId))
                    {
                        peopleEntered.Remove(personId);
                        PersonLeave(peopleLastFrame[personId]);
                        peopleLastFrame.Remove(personId);
                        if (peopleHistory.ContainsKey(personId))
                            peopleHistory.Remove(personId);
                    }
                }

                historyFrameCount++;
                if (historyFrameCount > 20)
                    historyFrameCount = 0;

            }

            this.depthImg.Source = this.depthBitmap;
            if (this.radioDepth.IsChecked == true)
                this.outImg.Source = this.depthBitmap;
            this.trackImg.Source = ImageHelpers.ToBitmapSource(trackImg);
            if (this.radioTrack.IsChecked == true)
                this.outImg.Source = this.trackImg.Source;
            this.diffImg.Source = ImageHelpers.ToBitmapSource(openCVImg);
            if (this.radioDiff.IsChecked == true)
                this.outImg.Source = this.diffImg.Source;
            if (background != null)
            {
                this.bgImg.Source = ImageHelpers.ToBitmapSource(background);
                if (this.radioBG.IsChecked == true)
                    this.outImg.Source = this.bgImg.Source;
            }
            txtBlobCount.Text = blobCount.ToString();

            if (blobCount == 0)
                backgroundValidation--;
            else if (backgroundValidation > 0)
            {
                backgroundValidation = BackgroundValidationFrameCount;
                CaptureBackground();
            }

            btnAutoCapture.IsEnabled = !(backgroundValidation > 0);
        }

        ArrayList PersonToArgs(Person person)
        {
            ArrayList args = new ArrayList();
            args.Add(person.id);
            args.Add(person.oid);
            args.Add(person.age);
            args.Add(person.centroidX);
            args.Add(person.centroidY);
            args.Add(person.velocityX);
            args.Add(person.velocityY);
            args.Add(person.depth);
            args.Add(person.boundingRectOriginX);
            args.Add(person.boundingRectOriginY);
            args.Add(person.boundingRectSizeWidth);
            args.Add(person.boundingRectSizeHeight);
            args.Add(person.highestX);
            args.Add(person.highestY);
            args.Add(person.haarRectX);
            args.Add(person.haarRectY);
            args.Add(person.haarRectWidth);
            args.Add(person.haarRectHeight);
            args.Add(person.opticalFlowVelocityX);
            args.Add(person.opticalFlowVelocityY);
            return args;
        }

        void PersonEnter(Person person)
        {
            if (udpWriter != null)
            {
                OSCPacket oscElement = new OSCMessage("/TSPS/personEntered/", PersonToArgs(person));
                udpWriter.Send(oscElement);
            }
        }

        void PersonUpdate(Person person)
        {
            if (udpWriter != null)
            {
                OSCMessage oscElement = new OSCMessage("/TSPS/personUpdated/", PersonToArgs(person));
                udpWriter.Send(oscElement);
            }
        }

        void PersonLeave(Person person)
        {
            if (udpWriter != null)
            {
                ArrayList args = new ArrayList();
                args.Add(person.id);
                OSCMessage oscElement = new OSCMessage("/TSPS/personWillLeave/", args);
                udpWriter.Send(oscElement);
            }
        }

        void AutoBackgroundCapture()
        {
            backgroundValidation = BackgroundValidationFrameCount;
            CaptureBackground();
        }

        void CaptureBackground()
        {
            if (latestDepth != null)
                background = latestDepth.Copy();

            if (background != null)
            {
                this.bgImg.Source = ImageHelpers.ToBitmapSource(background);
                if (radioBG.IsChecked == true)
                    this.outImg.Source = this.bgImg.Source;
            }
        }

        void OscConnect()
        {
            if (txtOscIP != null && txtOscPort != null)
                if (txtOscIP.Text != "" && txtOscPort.Text != "")
                {
                    ResetOsc();
                    udpWriter = new OSCTransmitter(txtOscIP.Text, int.Parse(txtOscPort.Text));
                    txtOscStatus.Text = "Osc Connected:\n" + txtOscIP.Text + ":" + txtOscPort.Text;
                }
        }

        void ResetOsc()
        {
            if (udpWriter != null)
                udpWriter.Close();
            udpWriter = null;
            txtOscStatus.Text = "Osc Disconnected";
        }

        void OscValidate(object sender, EventArgs e)
        {
            if (txtOscIP != null && txtOscPort != null)
                if (txtOscIP.Text == "" || txtOscPort.Text == "")
                {
                    chkOsc.IsChecked = false;
                    chkOsc.IsEnabled = false;
                }
                else
                {
                    chkOsc.IsEnabled = true;
                    if (chkOsc.IsChecked == true)
                    {
                        int port;
                        IPAddress ipAddress;
                        if (IPAddress.TryParse(txtOscIP.Text, out ipAddress) && int.TryParse(txtOscPort.Text, out port))
                            chkOsc_Checked(sender, (RoutedEventArgs)e);
                        else
                            chkOsc.IsChecked = false;
                    }
                }
        }

        #region Window Stuff
        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != proc)
            {
                if (!proc.HasExited)
                    proc.CloseMainWindow();
            }
            if (null != udpWriter)
            {
                udpWriter.Close();
            }
            stop = true;
            if (null != this.backgroundCaptureKey)
                this.backgroundCaptureKey.Dispose();
            if (null != this.exitKey)
                this.exitKey.Dispose();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundValidation > 0)
                backgroundValidation = 0;
            if (background != null)
                background.Dispose();
            background = null;
            this.bgImg.Source = null;
            if (radioBG.IsChecked == true)
                this.outImg.Source = null;
        }

        private void radioTrack_Checked(object sender, RoutedEventArgs e)
        {
            radioColor.IsChecked = false;
            radioDepth.IsChecked = false;
            radioBG.IsChecked = false;
            radioDiff.IsChecked = false;
            this.txtOut.Text = "Tracking";
        }

        private void radioDiff_Checked(object sender, RoutedEventArgs e)
        {
            radioColor.IsChecked = false;
            radioDepth.IsChecked = false;
            radioBG.IsChecked = false;
            radioTrack.IsChecked = false;
            this.txtOut.Text = "Differencing";
        }

        private void radioBG_Checked(object sender, RoutedEventArgs e)
        {
            radioColor.IsChecked = false;
            radioDepth.IsChecked = false;
            radioDiff.IsChecked = false;
            radioTrack.IsChecked = false;
            this.txtOut.Text = "Background";
            if (background != null)
                this.outImg.Source = ImageHelpers.ToBitmapSource(background);
        }

        private void radioColor_Checked(object sender, RoutedEventArgs e)
        {
            radioDepth.IsChecked = false;
            radioBG.IsChecked = false;
            radioDiff.IsChecked = false;
            radioTrack.IsChecked = false;
            this.txtOut.Text = "Color";
            this.outImg.Source = this.colorBitmap;
        }

        private void radioDepth_Checked(object sender, RoutedEventArgs e)
        {
            radioColor.IsChecked = false;
            radioBG.IsChecked = false;
            radioDiff.IsChecked = false;
            radioTrack.IsChecked = false;
            this.txtOut.Text = "Depth";
        }

        private void bgImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radioBG.IsChecked = true;
        }

        private void diffImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radioDiff.IsChecked = true;
        }

        private void trackImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radioTrack.IsChecked = true;
        }

        private void colorImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radioColor.IsChecked = true;
        }

        private void depthImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            radioDepth.IsChecked = true;
        }

        private void btnManualCapture_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundValidation > 0)
                backgroundValidation = 0;
            CaptureBackground();
        }

        private void btnAutoCapture_Click(object sender, RoutedEventArgs e)
        {
            AutoBackgroundCapture();
        }

        private void chkOsc_Checked(object sender, RoutedEventArgs e)
        {
            OscConnect();
        }

        private void chkOsc_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetOsc();
        }

        private void txtOscIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            OscValidate(sender, e);
        }

        private void txtOscPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            OscValidate(sender, e);
        }

        private void chkAutoMin_Checked(object sender, RoutedEventArgs e)
        {
            if (sliderMin != null)
                sliderMin.IsEnabled = false;
        }

        private void chkAutoMax_Checked(object sender, RoutedEventArgs e)
        {
            if (sliderMax != null)
                sliderMax.IsEnabled = false;
        }

        private void chkAutoMax_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sliderMax != null)
                sliderMax.IsEnabled = true;
        }

        private void chkAutoMin_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sliderMax != null)
                sliderMax.IsEnabled = true;
        }

        #endregion
    }
}
