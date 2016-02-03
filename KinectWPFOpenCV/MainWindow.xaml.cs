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
using Microsoft.Kinect;
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

namespace KinectWPFOpenCV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;
        DepthFrameReader depthReader;
        FrameDescription depthFrameDescription;
        WriteableBitmap depthBitmap;
        byte[] depthPixels;

        ColorFrameReader colorReader;
        FrameDescription colorFrameDescription;
        WriteableBitmap colorBitmap;

        Image<Bgr, Byte> background;
        Image<Bgr, Byte> latestDepth;

        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;
        const int BackgroundValidationFrameCount = 60;
        const int BackgroundOverlayFrameCount = 2;
        const double BackgroundOverlayFactor = 0.1;
        int backgroundValidation = BackgroundValidationFrameCount;

        byte[] colorPixels;

        int blobCount = 0;
        int frameCounter = 0;
        int bgFrameCounter = 0;
        double deltaTime = 0;
        DateTime lastFrame;

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
            this.sensor = KinectSensor.GetDefault();

            if (this.sensor != null)
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

        private void sensor_StatusChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.txtInfo.Text = e.IsAvailable ? "Running" : "Not Available";

            /*
            if (e.IsAvailable)
            {
                sensor_Initialize();
            }
            else
            {
                sensor_Stop();
            }
            */
        }

        private void sensor_Initialize()
        {
            if (null != this.sensor)
            {
                this.depthReader = this.sensor.DepthFrameSource.OpenReader();
                this.depthFrameDescription = this.sensor.DepthFrameSource.FrameDescription;
                this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
                this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

                this.depthReader.FrameArrived += this.sensor_DepthFramesReady;

                this.colorReader = this.sensor.ColorFrameSource.OpenReader();
                this.colorFrameDescription = this.sensor.ColorFrameSource.FrameDescription;
                this.colorPixels = new byte[this.colorFrameDescription.Width * this.colorFrameDescription.Height];
                this.colorBitmap = new WriteableBitmap(this.colorFrameDescription.Width, this.colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.colorImg.Source = this.colorBitmap;

                this.colorReader.FrameArrived += this.Reader_ColorFrameArrived;

                try
                {
                    this.sensor.Open();
                    this.sensor.IsAvailableChanged += sensor_StatusChanged;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
            backgroundValidation = BackgroundValidationFrameCount;
            this.outputViewbox.Visibility = System.Windows.Visibility.Visible;
            this.txtError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void sensor_Stop()
        {
            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
            if (null != this.depthReader)
            {
                this.depthReader.Dispose();
                this.depthReader = null;
            }
            if (null != this.colorReader)
            {
                this.colorReader.Dispose();
                this.colorReader = null;
            }
            sensor_NotReady();
        }

        private void sensor_NotReady()
        {
            this.outputViewbox.Visibility = System.Windows.Visibility.Collapsed;
            this.txtError.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void sensor_DepthFramesReady(object sender, DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {

                    if (chkAutoMin.IsChecked == true)
                        sliderMin.Value = depthFrame.DepthMinReliableDistance;

                    if (chkAutoMax.IsChecked == true)
                        sliderMax.Value = depthFrame.DepthMaxReliableDistance;

                    bool depthFrameProcessed = false;

                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }

                    if (depthFrameProcessed)
                    {
                        this.RenderDepthPixels();
                        blobCount = 0;
                        BlobDetection();
                    }
                }
            }

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
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
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
                        people[blobCount].depth = (float)(this.depthPixels[(int)centroidX + (int)centroidY * this.depthFrameDescription.Width]);
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
            if (null != this.sensor)
            {
                this.sensor.Close();
            }
            if (null != this.depthReader)
            {
                this.depthReader.Dispose();
                this.depthReader = null;
            }
            if (null != this.colorReader)
            {
                this.colorReader.Dispose();
                this.colorReader = null;
            }
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
