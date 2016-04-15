﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
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
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

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
        public MainWindow()
        {
            InitializeComponent();
            setting.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                ChildWindow chldWindow = new ChildWindow();
                chldWindow.ShowInTaskbar = false;
                chldWindow.Owner = Application.Current.MainWindow;
                chldWindow.ShowDialog();
                comport = chldWindow.comport;
                durationtime = chldWindow.durationtime;
                filename = chldWindow.fileName + ".dat";
                comportCont.Text = Convert.ToString(comport);
                durationtimeCont.Text = Convert.ToString(durationtime);
                filenameCont.Text = Convert.ToString(filename);
            });

            databaseWindowButton.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                DatabaseWindow dWindow = new DatabaseWindow();
                dWindow.ShowInTaskbar = false;
                dWindow.Owner = Application.Current.MainWindow;
                dWindow.ShowDialog();

            });


        }
        




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

        private void quitbutton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void restartbutton_Click(object sender, RoutedEventArgs e)
        {
            
            //this.sensor.Stop();
            //this.sensor.Start();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            vinkelImage.Source = null;
          

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
                            //this.DrawBonesAndJoints(skel, dc);
                            //Console.WriteLine("Innan calcvelocity");
                            this.CalculateVelocity(skel, dc);
                            //this.CalculateAngles(skel, dc);
                            Console.WriteLine("HEJ");
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

        
        // Create the MATLAB instance 
        MLApp.MLApp matlab = new MLApp.MLApp();


        private void stop_Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (this.sensor == null)
            {
                return;
            }
                  
            if (this.sensor.SkeletonStream.IsEnabled)
            {
                this.sensor.SkeletonStream.Disable();
            }

            if (this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.Disable();
            }
            this.sensor.SkeletonFrameReady -= this.SensorSkeletonFrameReady;
*/
            this.sensor.Stop();
        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
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
            _image.UriSource = new Uri( pathImage + @"\..\..\Vinkelgraf.jpeg", UriKind.RelativeOrAbsolute);          
            _image.EndInit();
        
             vinkelImage.Source = _image;
        }
        /*
        private void CompositionTargetRendering1() //object sender, EventArgs e
        {
            BitmapImage image = new BitmapImage();
            string pathImage = Path.Combine(Directory.GetCurrentDirectory());

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.None;
            image.UriCachePolicy = new System.Net.Cache.RequestCachePolicy();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(pathImage + @"\..\..\Vinkelgraf.jpeg", UriKind.RelativeOrAbsolute);
            image.EndInit();

            vinkelImage.Source = image;
        }*/

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// 

        //------------------------------- Hastighetsberäkning -----------------------------------// 
        // Variabler för hastighet
        double stepTime = 0;
        double sumStep = 0;
        double stepVelocity = 0;
        double meanVelocity = 0;
        double meanHelpVelocity = 0;

        // Listor för hastighet
        List<double> velXList = new List<double>();
        List<double> velTList = new List<double>();
        List<double> velHelpList = new List<double>();
        List<double> velocityList = new List<double>();

        int count1 = 0;

        // Beräkna hastigheten 
        // Håll listan till 600 värden
        // När jag hittar vändpunkt så tömmer jag listan 
        private void CalculateVelocity(Skeleton skeleton, DrawingContext drawingContext)
        {
            //Koordinater för fot
            Joint footLeft = skeleton.Joints[JointType.FootLeft];
            float XFootleft;
            float YFootleft;
            XFootleft = footLeft.Position.X;
            YFootleft = footLeft.Position.Y;

            ++count1;
            if (count1 == 600)
            {
                count1 = 0;
            }

            //Lägger till x-koordinater i listan
            if (velXList.Count > 10)
            {
                velXList.Add(XFootleft);

                if (velXList.Count < 399)
                {

                    // Kollar om ett värde är en topp genom att jämföra ett värden före sig och ett värde efter sig
                    if (((velXList[velXList.Count - 5] < ((velXList[velXList.Count - 4] + velXList[velXList.Count - 3] + velXList[velXList.Count - 2] + velXList[velXList.Count - 1]) / 4))
                        &&
                        (velXList[velXList.Count - 5] < ((velXList[velXList.Count - 6] + velXList[velXList.Count - 7] + velXList[velXList.Count - 8] + velXList[velXList.Count - 9]) / 4)))
                        ||
                       ((velXList[velXList.Count - 5] > ((velXList[velXList.Count - 4] + velXList[velXList.Count - 3] + velXList[velXList.Count - 2] + velXList[velXList.Count - 1]) / 4))
                        &&
                        (velXList[velXList.Count - 5] > ((velXList[velXList.Count - 6] + velXList[velXList.Count - 7] + velXList[velXList.Count - 8] + velXList[velXList.Count - 9]) / 4))))
                    {
                        //Counterräknare
                        countertext.Text = Convert.ToString(count1);

                        velHelpList.Add(velXList[velXList.Count - 5]);

                        //Vilken x-koordinat som sparas
                        initX.Text = Convert.ToString(velXList[velXList.Count - 5]);

                        velhelptext.Text = Convert.ToString(velHelpList.Count);

                        //Beräknar delta-x
                        sumStep = sumStep + Math.Abs(Math.Abs(velXList[velXList.Count - 5]) - Math.Abs(velXList[velXList.Count - 6]));

                        // Delta-tid
                        stepTime = velHelpList.Count / 30;

                        steptimetext.Text = Convert.ToString(stepTime);

                        // Steghastighet
                        stepVelocity = sumStep / stepTime;

                        // Hastighetslista
                        velocityList.Add(stepVelocity);
                        hastighetslista.Text = Convert.ToString(velocityList.Count);
                        meanHelpVelocity = meanHelpVelocity + stepVelocity;
                        meanVelocity = velocityList.Count / meanHelpVelocity;

                        // Skriver ut hastigheten i fönstret
                        velocityText.Text = Convert.ToString(meanVelocity);

                        // Rensar listorna
                        //velXList.Clear();
                        velHelpList.Clear();
                    }
                    else
                    {
                        // Lägger till värdet i en lista för att senare kunna räkna ut tiden
                        // Samt summerar längden av varje delta-x
                        velHelpList.Add(velXList.Count - 5);
                        sumStep = sumStep + Math.Abs(Math.Abs(velXList[velXList.Count - 5]) - Math.Abs(velXList[velXList.Count - 6]));
                    }
                }
                else
                {
                    // Tar bort första sista talet i velXlist, samt adderar en ny x-koordinat
                    velXList.RemoveAt(0);
                }
            }
            else
            {
                velXListtext.Text = Convert.ToString(velXList.Count);
                velXList.Add(XFootleft);
            }

            if (velocityList.Count > 100)
            {
                velocityList.RemoveAt(0);
            }
        }

        int count = 0;

        // -------------------------------------------------------------------------------------//
        //------------------------------- Vinkelberäkning --------------------------------------//
        // -------------------------------------------------------------------------------------//

        // Skapar listorna som behövs
        public List<double> vinklar = new List<double>();
        public List<double> tidsLista = new List<double>();
        public List<double> minimumlista = new List<double>();
        public List<double> minimumlistahelp = new List<double>();
        //public List<double> meanAngleList = new List<double>();

       // Skapar variablerna som behövs
        public double helprefresh = 30;
        public double sampleToTime = 0;
        public double lagsta_varde;
        public int updateMatlab = 0;
        public double meanAngle = 170;
 
        // Beräknar vinklar beroende på checkboxar
        void CalculateAngles(Skeleton skeleton, DrawingContext drawingcontext)
        {
            // Definerar jointar
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

            //-------Endast för kontroll om den anropas------//
            ++count1;
            if (count1 == 600)
            {
                count1 = 0;
            }
            //-----------------------------------------------// 

            // Om båda checkboxarna är ifyllda så slängs ett felmeddelande och boxarna töms
            if ((bool)SHKbox.IsChecked && (bool)FHKbox.IsChecked)
            {
                felmeddelande.Text = "Det går endast att mäta en vinkel åt gången!";
                SHKbox.IsChecked = false;
                FHKbox.IsChecked = false;
            }

            // Kollar om checkbox är ifylld
            if ((bool)SHKbox.IsChecked)
            {
                felmeddelande.Text = "";

                double HipKnee_Length = Math.Sqrt(Math.Pow(XHipleft - XKneeleft, 2) + Math.Pow(YHipleft - YKneeleft, 2));
                double HipShoulder_Length = Math.Sqrt(Math.Pow(XHipleft - XShoulderleft, 2) + Math.Pow(YHipleft - YShoulderleft, 2));
                double KneeShoulder_Length = Math.Sqrt(Math.Pow(XKneeleft - XShoulderleft, 2) + Math.Pow(YKneeleft - YShoulderleft, 2));

                //SHK - Cosinussatsen för vinkel Höft-knä-fot, avrundar till heltal
                double SHK_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(HipShoulder_Length, 2)
                    - Math.Pow(KneeShoulder_Length, 2)) / (2 * HipKnee_Length * HipShoulder_Length))) * (180 / Math.PI));

                vinklar.Add(SHK_angle);
                minimumlistahelp.Add(SHK_angle);

                if (SHK_angle < 140)
                {
                    textTestdirektiv.Text = "Sträck på dig!!!";
                    SystemSounds.Asterisk.Play();
                }
                else
                {
                    textTestdirektiv.Text = "";
                }
            }

            // Kollar om checkbox är ifylld
            if ((bool)FHKbox.IsChecked)
            {
                felmeddelande.Text = "";

                //vektorlängder
                double HipKnee_Length = Math.Sqrt(Math.Pow(XHipleft - XKneeleft, 2) + Math.Pow(YHipleft - YKneeleft, 2));
                double HipFoot_Length = Math.Sqrt(Math.Pow(XHipleft - XFootleft, 2) + Math.Pow(YHipleft - YFootleft, 2));
                double KneeFoot_Length = Math.Sqrt(Math.Pow(XKneeleft - XFootleft, 2) + Math.Pow(YKneeleft - YFootleft, 2));

                //FHK - cosinussatsen för vinkel Höft-knä-fot, avrundar till heltal
                double FHK_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(KneeFoot_Length, 2)
                    - Math.Pow(HipFoot_Length, 2)) / (2 * HipKnee_Length * KneeFoot_Length))) * (180 / Math.PI));

                vinklar.Add(FHK_angle);
                minimumlistahelp.Add(FHK_angle);

                if (FHK_angle < 90)
                {
                    textTestdirektiv.Text = "Sträck ut i knäna!!!";
                    SystemSounds.Asterisk.Play();
                }
                else
                { 
                    textTestdirektiv.Text = "";
                }
            }

            sampleToTime = vinklar.Count;
            tidsLista.Add(sampleToTime / 30);

            // tar ut lägsta vinkel
            if (minimumlistahelp.Count > 60)
            {
                lagsta_varde = minimumlistahelp.Min();
                minimumlista.Add(lagsta_varde);
                minimumlistahelp.RemoveAt(0);
            }
            else
            {
                minimumlista.Add(lagsta_varde);
            }
            //printMatLab(tidsLista, vinklar, minimumlista);
        }

 // -------------------------------------------------------------------------------------//
 // -------------------------- Saker som ritar ------------------------------------------//
 // -------------------------------------------------------------------------------------//

        // Skickar allting till matlab och plottas sedan
        void printMatLab(List<double> list1, List<double> list2, List<double> list3)
        {
            //MATLABPLOT
            //Skickar data till matlab i ett specifikt satt intervall
            if (updateMatlab < vinklar.Count)
            {
                // Change to the directory where the function is located 
                var path = Path.Combine(Directory.GetCurrentDirectory());
                matlab.Execute(@"cd " + path + @"\..\..");

                // Define the output 
                object result = null;

                // Call the MATLAB function myfunc! Kastar även eventuella runtimefel
                try
                {
                    CompositionTargetRendering();
                    matlab.Feval("myfunc", 1, out result, list1.ToArray(), list2.ToArray(), list3.ToArray());
                }
                catch (System.Runtime.InteropServices.COMException)
                {

                }
                updateMatlab = updateMatlab + 30;
            }
        }

        // Skickar allting till matlab och plottas sedan
        void printMatLab1(string funktionsnamn, string comport, int durationtime, string fileName)
        {
            //MATLABPLOT
            //Skickar data till matlab i ett specifikt satt intervall
            
                // Change to the directory where the function is located 
                var path = Path.Combine(Directory.GetCurrentDirectory());
                matlab.Execute(@"cd " + path + @"\..\..");

                // Define the output 
                object result = null;

                // Call the MATLAB function myfunc! Kastar även eventuella runtimefel
                try
                {
                    matlab.Feval(funktionsnamn, 1, out result, comport.ToString(), durationtime, fileName);

                }
                catch (System.Runtime.InteropServices.COMException)
                {

                }
        }

        // Ritar ut skelettmodellen på bilden
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


        // För att ta ut medelvinkel
        private void Show_mean_angle_Click(object sender, RoutedEventArgs e)
        {
            minimumlista.Sort();
            int listIndex = 0;
            while(listIndex < minimumlista.Count - 1)
            {
                if(minimumlista[listIndex] == minimumlista[listIndex +1])
                {
                    minimumlista.RemoveAt(listIndex);
                }
                else
                {
                    ++listIndex;
                }
            }
            foreach(var tal in minimumlista)
            {
                meanAngle = meanAngle + tal;
            }
            meanAngle = meanAngle / (minimumlista.Count);
            Math.Ceiling(meanAngle);

            System.Windows.MessageBox.Show(meanAngle.ToString());
            
        }

        private void FKHbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SHKbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        
        public string comport = null; 
        public int durationtime = 0;
        public string filename = null; 

        private void display_heartrate_Click(object sender, RoutedEventArgs e)
        {
            //printMatLab1("ecgtoheartrate", comport, durationtime, filename);
        }

        private void display_angle_Click(object sender, RoutedEventArgs e)
        {
            printMatLab(tidsLista, vinklar, minimumlista);
        }
        
        private void setting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}