//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
//
// Kommentarer på svenska följs av kod som är implementerad av RoadRunners
// Kommentarer på engelska följs av baskod från SDK
//
//------------------------------------------------------------------------------




namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using Microsoft.Kinect;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using System.Media;
    using System.Collections;
    using System.Threading;
    using System.Globalization;
    using System.Windows.Threading;
    using System.Diagnostics;
    using System.Timers;





    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Red, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 

        // Variabeldefinitioner
        private SaveData saveData;
        public string comport = null;
        public int durationtime;
        public string bufferFile = "buffer.dat";

        //Definitioner av olika trådar
        Thread heartrateThread;
        Thread plotAnglesThread;

        public MainWindow()
        {
            InitializeComponent();

            // Skapar ett extra fönster
            setting.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                ChildWindow chldWindow = new ChildWindow();
                chldWindow.ShowInTaskbar = false;
                chldWindow.Owner = Application.Current.MainWindow;
                chldWindow.ShowDialog();

                comport = chldWindow.comport;

                comportCont.Text = Convert.ToString(comport);

            });

            this.saveData = new SaveData();

        }
        /*
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

        }
        */

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        // Stänger programmet
        private void quitbutton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Startar om programmet
        private void restartbutton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }


        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            vinkelImage.Source = null;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
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
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.ColorStream.Enable();
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                            //Console.WriteLine("Innan calcvelocity");
                            this.CalculateVelocity(skel, dc);
                            this.CalculateAngles(skel, dc);
                            if (changeButton == 0)
                            {
                                this.ClickStartButtonGesture(skel);
                            }

                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        //Hämtar bild som ritas i matlab
        private void CompositionTargetRendering() //object sender, EventArgs e
        {
            BitmapImage _image = new BitmapImage();
            string pathImage = Path.Combine(Directory.GetCurrentDirectory());

            _image.BeginInit();
            _image.CacheOption = BitmapCacheOption.None;
            _image.UriCachePolicy = new System.Net.Cache.RequestCachePolicy();
            _image.CacheOption = BitmapCacheOption.OnLoad;
            _image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            _image.UriSource = new Uri(pathImage + @"\..\..\Vinkelgraf.jpeg", UriKind.RelativeOrAbsolute);
            _image.EndInit();

            vinkelImage.Source = _image;
        }

        int changeButton = 0;

       private void ClickStartButtonGesture(Skeleton skeleton)
        {
            //Koordinater för höger hand
            Joint handRight = skeleton.Joints[JointType.HandRight];
            Joint handLeft = skeleton.Joints[JointType.HandLeft];

            float XHandRight;
            float YHandRight;
            float YHandLeft;
            float XHandLeft;

            XHandRight = handRight.Position.X;
            YHandRight = handRight.Position.Y;
            XHandLeft = handLeft.Position.X;        
            YHandLeft = handLeft.Position.Y;

            if (XHandRight > 0.8 && YHandRight > 0.55)
            {
                startLoggingButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                changeButton = 1;
            }

            //if (XHandLeft < -0.65 && YHandLeft > 0.55)
            //{
            //    restartbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            //}
        }



        //---------------------------------------------------------------------------------------//
        //------------------------------- Hastighetsberäkning -----------------------------------//
        //---------------------------------------------------------------------------------------//

        // Variabler för hastighet
        double deltaT = 0;
        double deltaX = 0;
        double stepVelocity = 0;
        double maxValuePlotHelp = 0;
        int velocitycount = 0;

        // Listor för hastighet
        List<double> velXList = new List<double>();
        List<double> velHelpListPlot = new List<double>();
        List<double> velHelpListSave = new List<double>();
        List<double> velocityList = new List<double>();
        List<double> velocityListSave = new List<double>();
        List<double> velocityListDatabase = new List<double>();

        // Beräknar hastigheten genom att ta skillnaden mellan fyra på varandra följande värden och dividerar sedan
        // det med tidsskillnaden. Hastigheten = deltaX / deltaT.
        private void CalculateVelocity(Skeleton skeleton, DrawingContext drawingContext)
        {
            //Koordinater för fot
            Joint footLeft = skeleton.Joints[JointType.FootLeft];
            float XFootleft;
            float YFootleft;
            XFootleft = footLeft.Position.X;
            YFootleft = footLeft.Position.Y;
            velXList.Add(XFootleft);

            //Lägger till x-koordinater i listan
            if (velXList.Count > 30)
            {
                // Räknar ut avståndsskillnaden mellan 4 sampelvärden
                deltaX = Math.Abs(velXList[velXList.Count - 1]) - Math.Abs(velXList[velXList.Count - 4]);
                deltaX = Math.Abs(deltaX);
            }
            // Skillnaden i tid mellan sampelvärdena
            deltaT = 0.1;
            stepVelocity = deltaX / deltaT;
            velocityList.Add(stepVelocity);

            // Det som plottas i fönstret
            if (velocityList.Count == 15)
            {
                double maxValuePlot = velocityList.Max();
                velHelpListPlot.Add(maxValuePlot);
                velocityList.Clear();
            }

            if (velHelpListPlot.Count == 4)
            {
                maxValuePlotHelp = velHelpListPlot.Average();
                initVel.Text = Convert.ToString(Math.Floor(maxValuePlotHelp * 3.6)) + " km/h"; //Här ska det läggas till round
                velHelpListPlot.Clear();
            }

            if (velXList.Count > 600)
            {
                velXList.RemoveAt(0);
            }
        }

        // Det som sparas ner till databasen
        public void CalculateVelocitySave()
        {
            velocityListSave.Add(Math.Round(maxValuePlotHelp * 3.6, 2));
            ++velocitycount;

            if (velocitycount == k)
            {
                double meanVelocity = velocityListSave.Average();
                velocityListDatabase.Add(meanVelocity);
                velocityListSave.Clear();
                velocitycount = 0;
            }
            saveData.ExcelVelocityFunk(velocityListDatabase);
        }


        // -------------------------------------------------------------------------------------//
        //------------------------------- Vinkelberäkning --------------------------------------//
        // -------------------------------------------------------------------------------------//

        // Skapar listorna som behövs för FHK
        public List<double> angles_FHK = new List<double>();
        public List<double> minimumlista_FHK = new List<double>();
        public List<double> anglesHelp_FHK = new List<double>();
        public List<double> meanList_FHK = new List<double>();

        // Skapar listorna som behövs för SHK
        public List<double> angles_SHK = new List<double>();
        public List<double> minimumlista_SHK = new List<double>();
        public List<double> anglesHelp_SHK = new List<double>();
        public List<double> meanList_SHK = new List<double>();

        // Skapar listorna som behövs för puls
        public List<double> pulseList = new List<double>();
        public List<double> minimumList_pulse = new List<double>();
        public List<double> pulseListHelp = new List<double>();
        public List<double> meanList_pulse = new List<double>();

        //Lista för tiden 
        public List<double> timeList = new List<double>();
        public double timeTick = 0;

        // Skapar variablerna som behövs
        public double lagsta_varde_FHK = 0;
        public double lagsta_varde_SHK = 0;
        public int updateMatlab = 0;
        public double meanAngle_FHK;
        public double meanAngle_SHK;
        public double meanPulse;
        public double lagsta_varde_Puls;
        int i = 0;
        int k = 0;

            // Plockar ut medelvärden utifrån valt intervall
        public void meanAngleFunc(List<double> minList1, List<double> minList2, List<double> minList3)
        {

            if (i == k)
            {
                meanAngle_FHK = minList1.Average();
                meanList_FHK.Add(meanAngle_FHK);
                meanAngleBlock_FHK.Text = Convert.ToString(Math.Ceiling(meanList_FHK.LastOrDefault())) + (char)176;
                minimumlista_FHK.Clear();

                meanAngle_SHK = minList2.Average();
                meanList_SHK.Add(meanAngle_SHK);
                meanAngleBlock_SHK.Text = Convert.ToString(Math.Ceiling(meanList_SHK.LastOrDefault())) + (char)176;
                minimumlista_SHK.Clear();

                meanPulse = minList3.Average();
                meanList_pulse.Add(meanPulse);
                pulstest.Text = Convert.ToString(Math.Ceiling(meanList_pulse.LastOrDefault())) + " BPM";
                minimumList_pulse.Clear();

                timeList.Add(timeTick);

                saveData.ExcelFunkFHK(meanList_FHK);
                saveData.ExcelFunkSHK(meanList_SHK);
                saveData.ExcelPulseFunk(meanList_pulse);

                i = 0;
            }
        }

        // Beräknar vinklar 
        public void CalculateAngles(Skeleton skeleton, DrawingContext drawingcontext)
        {
            plotAngles();

            // Definerar leder
            Joint kneeLeft = skeleton.Joints[JointType.KneeLeft];
            Joint hipLeft = skeleton.Joints[JointType.HipLeft];
            Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
            Joint footLeft = skeleton.Joints[JointType.FootLeft];

            float XFootleft;
            float YFootleft;
            float XKneeleft;
            float YKneeleft;
            float XHipleft;
            float YHipleft;
            float XShoulderleft;
            float YShoulderleft;

            //Koordinater för knä, höft, axel
            XFootleft = footLeft.Position.X;
            YFootleft = footLeft.Position.Y;
            XKneeleft = kneeLeft.Position.X;
            YKneeleft = kneeLeft.Position.Y;
            XHipleft = hipLeft.Position.X;
            YHipleft = hipLeft.Position.Y;
            XShoulderleft = shoulderLeft.Position.X;
            YShoulderleft = shoulderLeft.Position.Y;

            double HipKnee_Length = Math.Sqrt(Math.Pow(XHipleft - XKneeleft, 2) + Math.Pow(YHipleft - YKneeleft, 2));
            double HipShoulder_Length = Math.Sqrt(Math.Pow(XHipleft - XShoulderleft, 2) + Math.Pow(YHipleft - YShoulderleft, 2));
            double KneeShoulder_Length = Math.Sqrt(Math.Pow(XKneeleft - XShoulderleft, 2) + Math.Pow(YKneeleft - YShoulderleft, 2));
            double HipFoot_Length = Math.Sqrt(Math.Pow(XHipleft - XFootleft, 2) + Math.Pow(YHipleft - YFootleft, 2));
            double KneeFoot_Length = Math.Sqrt(Math.Pow(XKneeleft - XFootleft, 2) + Math.Pow(YKneeleft - YFootleft, 2));

            //SHK - Cosinussatsen för vinkel Spine-hip-knee, avrundar till heltal
            double SHK_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(HipShoulder_Length, 2)
                - Math.Pow(KneeShoulder_Length, 2)) / (2 * HipKnee_Length * HipShoulder_Length))) * (180 / Math.PI));

            //FHK - Cosinussatsen för vinkel höft-knä-fot, avrundar till heltal
            double FHK_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(KneeFoot_Length, 2)
                    - Math.Pow(HipFoot_Length, 2)) / (2 * HipKnee_Length * KneeFoot_Length))) * (180 / Math.PI));



            //Kollar så SHK vinkeln inte är NaN (not a number)
            if (Double.IsNaN(SHK_angle))
            {
                double prevValSHKlist = angles_SHK[angles_SHK.Count - 1];
                anglesHelp_SHK.Add(prevValSHKlist);
                angles_SHK.Add(prevValSHKlist);
            }
            else
            {
                angles_SHK.Add(SHK_angle);
                anglesHelp_SHK.Add(SHK_angle);
            }

            if (SHK_angle < 140)
            {
                textTestdirektiv.Text = "Stand up straight!";
                SystemSounds.Asterisk.Play();
            }
            else
            {
                textTestdirektiv.Text = "";
            }

            //Kollar så FHK vinkeln inte är NaN
            if (Double.IsNaN(FHK_angle))
            {
                double prevValFHKlist = angles_FHK[angles_FHK.Count - 1];
                anglesHelp_FHK.Add(prevValFHKlist);
                angles_FHK.Add(prevValFHKlist);
            }
            else
            {
                angles_FHK.Add(FHK_angle);
                anglesHelp_FHK.Add(FHK_angle);
            }
            if (FHK_angle < 90)
            {
                textTestdirektiv.Text = "pull out your knees!";
                SystemSounds.Asterisk.Play();
            }
            else
            {
                textTestdirektiv.Text = "";
            }


            if (counter2 >= 2)
            {
                readPulseData();
                CalculateVelocitySave();

                //Knävinkel
                lagsta_varde_FHK = anglesHelp_FHK.Min();
                minimumlista_FHK.Add(lagsta_varde_FHK);
                anglesHelp_FHK.Clear();

                //Höftvinkel
                lagsta_varde_SHK = anglesHelp_SHK.Min();
                minimumlista_SHK.Add(lagsta_varde_SHK);
                anglesHelp_SHK.Clear();

                //pulsen
                lagsta_varde_Puls = pulseListHelp.Min();
                minimumList_pulse.Add(lagsta_varde_Puls);
                pulseListHelp.Clear();

                //Tar ut medelvinklar
                meanAngleFunc(minimumlista_FHK, minimumlista_SHK, minimumList_pulse);

                counter2 = 0;
                i++;

            }
        }





        // -------------------------------------------------------------------------------------//
        // -------------------------- Saker som ritar ------------------------------------------//
        // -------------------------------------------------------------------------------------//

        // Startar en matlabinstans
        MLApp.MLApp matlab = new MLApp.MLApp();

        // Skickar allting till matlab och plottas sedan
        public void printMatLab(List<double> list1, List<double> list2, List<double> list3, List<double> list4)
        {
            // Change to the directory where the function is located 
            var path = Path.Combine(Directory.GetCurrentDirectory());
            matlab.Execute(@"cd " + path + @"\..\..");

            // Define the output 
            object result = null;


            // Call the MATLAB function myfunc! Kastar även eventuella runtimefel
            try
            {

                matlab.Feval("myfunc", 1, out result, list1.ToArray(), list2.ToArray(), list3.ToArray(), list4.ToArray());
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("Listorna har ej samma längd :(");
            }
        }


        // Skickar allting till matlab och plottas sedan
        public void printMatLab1(string funktionsnamn, string comport, int durationtime, string filename)
        {
            // Change to the directory where the function is located 
            var path = Path.Combine(Directory.GetCurrentDirectory());
            matlab.Execute(@"cd " + path + @"\..\..");

            // Define the output 
            object result = null;

            // Call the MATLAB function myfunc! Kastar även eventuella runtimefel
            try
            {
                matlab.Feval(funktionsnamn, 0, out result, comport.ToString(), durationtime, filename);
            }

            catch (System.Runtime.InteropServices.COMException)
            {

            }
        }


        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
 
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {


            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }


        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>


        // -------------------------------------------------------------------------------------//
        // --------------------------------- Vinkel på kinecten --------------------------------// 
        // -------------------------------------------------------------------------------------//

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int n = (int)slider.Value;

            Degree.Content = n.ToString();
        }

        private void Kinect_angle_Click(object sender, RoutedEventArgs e)
        {
            if (sensor.ElevationAngle != (int)slider.Value)
            {
                sensor.ElevationAngle = (int)slider.Value;
            }
        }



        private void display_heartrate_Click(object sender, RoutedEventArgs e)
        {

            durationtime = saveData.ReturnTestLength() + 30;

            if (comport == "")
            {
                MessageBox.Show("Add heartrate settings");
            }
            else
            {
                heartrateThread = new Thread(() => printMatLab1("heartRateCalc", comport, durationtime, bufferFile));
                heartrateThread.Start();
            }
        }

        private void display_angle_Click(object sender, RoutedEventArgs e)
        {
            CompositionTargetRendering();
            plotAnglesThread = new Thread(() => printMatLab(timeList, meanList_pulse, meanList_FHK, meanList_SHK));
            plotAnglesThread.Start();

        }





        private void readPulseData()
        {
            try
            {
                var currentpath = Path.Combine(Directory.GetCurrentDirectory());
                String line = File.ReadAllLines(currentpath + @"\..\..\pulsdata1.txt").LastOrDefault();
                double pulsTodec = Double.Parse(line, NumberStyles.Float, CultureInfo.InvariantCulture);


                pulseList.Add(Math.Ceiling(pulsTodec));
                pulseListHelp.Add(Math.Ceiling(pulsTodec));

            }

            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

        }


        public void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;

            // ... Set SelectedItem as Window Title.
            string BoxValue = comboBox.SelectedItem as string;
            saveData.ExcelFunkIntervall(comboBox.SelectedIndex);

            if (comboBox.SelectedIndex == 0)
            {
                k = 1;
            }
            if (comboBox.SelectedIndex == 1)
            {
                k = 5;
            }
            if (comboBox.SelectedIndex == 2)
            {
                k = 10;
            }
            if (comboBox.SelectedIndex == 3)
            {
                k = 30;
            }
        }
        public int plotCounter = 2;
        public void plotAngles()
        {

            if (timeList.Count > plotCounter)
            {
                CompositionTargetRendering();
                plotAnglesThread = new Thread(() => printMatLab(timeList, meanList_pulse, meanList_FHK, meanList_SHK));
                plotAnglesThread.Start();

                plotCounter = plotCounter + 2;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("2 Seconds Interval");
            data.Add("10 Seconds Interval");
            data.Add("20 Seconds Interval");
            data.Add("60 Seconds Interval");

            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;

        }

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private int counter;
        public int counter2 = 0;


        private void startLoggingButton_Click(object sender, EventArgs e)
        {
            meanList_SHK.Clear();
            meanList_FHK.Clear();
            velocityListDatabase.Clear();

            counter = saveData.ReturnTestLength();

            counter2 = 0;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();
            timerContent.Text = counter.ToString();

            timer2 = new System.Windows.Forms.Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 1000; // 1 second
            timer2.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            timeTick++;

            if (counter == 0)
            {
                timer1.Stop();
                changeButton = 0;
            }
            timerContent.Text = counter.ToString();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            counter2++;
            // timer2.Stop();
        }


        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox1 = sender as ComboBox;

            // ... Set SelectedItem as Window Title.
            string BoxValue = comboBox1.SelectedItem as string;
            saveData.ExcelTestLength(comboBox1.SelectedIndex);
        }

        //public int testLength;

        public void ComboBox1_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("10 Seconds Test");
            data.Add("30 Seconds Test");
            data.Add("60 Seconds Test");
            data.Add("5 Minutes Test");
            data.Add("10 Minutes Test");
            data.Add("30 Minutes Test");
            data.Add("60 Minutes Test");

            // ... Get the ComboBox reference.
            var comboBox1 = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox1.ItemsSource = data;

            // ... Make the first item selected.
            comboBox1.SelectedIndex = 0;

        }

        private void Save_button_click(object sender, RoutedEventArgs e)
        {
            saveData.Show();
        }

        private void ResetTest_button_Click(object sender, RoutedEventArgs e)
        {
            timer2.Stop();
            timer1.Stop();
            timerContent.Text = "0";
            meanAngleBlock_FHK.Text = "0";
            meanAngleBlock_SHK.Text = "0";
            initVel.Text = "0";
            pulstest.Text = "0";
            timeList.Clear();
            meanList_pulse.Clear();
            meanList_FHK.Clear();
            meanList_SHK.Clear();
            velocityListDatabase.Clear();
        }
    }
}






