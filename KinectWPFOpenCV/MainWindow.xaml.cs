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

namespace KinectWPFOpenCV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;
        WriteableBitmap depthBitmap;
        WriteableBitmap colorBitmap;
        DepthImagePixel[] depthPixels;
        Image<Bgr, Byte> background;
        Image<Bgr, Byte> latestDepth;

        int backgroundValidation = 60;

        byte[] colorPixels;

        int blobCount = 0;

        public MainWindow()
        {
            InitializeComponent();
           
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.MouseDown += MainWindow_MouseDown;

        }

      
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }


            if (null != this.sensor)
            {

                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.colorImg.Source = this.colorBitmap;

                this.sensor.AllFramesReady += this.sensor_AllFramesReady;

                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.outputViewbox.Visibility = System.Windows.Visibility.Collapsed;
                this.txtError.Visibility = System.Windows.Visibility.Visible;
                this.txtInfo.Text = "No Kinect Found";
                
            }

        }

        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            BitmapSource depthBmp = null;
            blobCount = 0;

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame != null)
                    {

                        blobCount = 0;

                        depthBmp = depthFrame.SliceDepthImage((int)sliderMin.Value, (int)sliderMax.Value);
                        
                        Image<Bgr, Byte> openCVImg = new Image<Bgr, byte>(depthBmp.ToBitmap());

                        if (latestDepth != null)
                            latestDepth.Dispose();
                        latestDepth = openCVImg.Clone();

                        if (background != null)
                        {
                            openCVImg -= background;
                        }

                        openCVImg._GammaCorrect(sliderGamma.Value);
                        openCVImg = openCVImg.ThresholdToZero(new Bgr(255 - sliderThreshold.Value, 255 - sliderThreshold.Value, 255 - sliderThreshold.Value));

                        Image<Bgr, Byte> trackImg = new Image<Bgr, byte>(depthBmp.PixelWidth, depthBmp.PixelHeight);
                        Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();

                        using (MemStorage stor = new MemStorage())
                        {
                            //Find contours with no holes try CV_RETR_EXTERNAL to find holes
                            Contour<System.Drawing.Point> contours = gray_image.FindContours(
                             Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                             Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                             stor);

                            for (int i = 0; contours != null; contours = contours.HNext)
                            {
                                i++;

                                if ((contours.Area > Math.Pow(sliderMinSize.Value, 2)) && (contours.Area < Math.Pow(sliderMaxSize.Value, 2)))
                                {
                                    double centroidX = contours.BoundingRectangle.Location.X + contours.BoundingRectangle.Width / 2;
                                    double centroidY = contours.BoundingRectangle.Location.Y + contours.BoundingRectangle.Height / 2;
                                    //Contours
                                    trackImg.Draw(contours, new Bgr(System.Drawing.Color.WhiteSmoke), 1);
                                    //Centroid
                                    trackImg.Draw(new Cross2DF(new PointF((float)centroidX, (float)centroidY), 10, 10), new Bgr(System.Drawing.Color.Yellow), 2);
                                    //BoundingRectangle
                                    trackImg.Draw(contours.BoundingRectangle, new Bgr(System.Drawing.Color.Yellow), 2);
                                    //BoundingRectangle Origin
                                    trackImg.Draw(new Cross2DF(new PointF((float)contours.BoundingRectangle.Location.X, (float)contours.BoundingRectangle.Location.Y), 10, 10), new Bgr(System.Drawing.Color.Green), 2);
                                    //MinAreaRect
                                    //MCvBox2D box = contours.GetMinAreaRect();
                                    //trackImg.Draw(box, new Bgr(System.Drawing.Color.Yellow), 2);
                                    //Create the font
                                    MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1.0, 1.0);
                                    trackImg.Draw("id: " + blobCount.ToString(), ref f, new System.Drawing.Point((int)centroidX + 10, (int)centroidY + 5), new Bgr(System.Drawing.Color.Yellow));
                                    blobCount++;
                                }
                            }
                        }

                        this.depthImg.Source = depthBmp;
                        if (this.radioDepth.IsChecked == true)
                            this.outImg.Source = depthBmp;
                        this.trackImg.Source = ImageHelpers.ToBitmapSource(trackImg);
                        if (this.radioTrack.IsChecked == true)
                            this.outImg.Source = ImageHelpers.ToBitmapSource(trackImg);
                        this.diffImg.Source = ImageHelpers.ToBitmapSource(openCVImg);
                        if (this.radioDiff.IsChecked == true)
                            this.outImg.Source = ImageHelpers.ToBitmapSource(openCVImg);
                        txtBlobCount.Text = blobCount.ToString();

                        if (blobCount == 0)
                            backgroundValidation--;
                        else if(backgroundValidation > 0)
                            CaptureBackground();
                    }
                }


                if (colorFrame != null)
                {
                    
                      colorFrame.CopyPixelDataTo(this.colorPixels);
                      this.colorBitmap.WritePixels(
                          new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                          this.colorPixels,
                          this.colorBitmap.PixelWidth * sizeof(int),
                          0);
                    
                }
            }
        }

        void CaptureBackground()
        {
            if (latestDepth != null)
                background = latestDepth.Clone();

            this.bgImg.Source = ImageHelpers.ToBitmapSource(background);
        }


        #region Window Stuff
        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            CaptureBackground();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (background != null)
                background.Dispose();
            background = null;
            this.depthImg.Source = null;
        }

        #endregion

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
    }
}
