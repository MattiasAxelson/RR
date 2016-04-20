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
    using System.Collections;
    using System.Threading;
    using System.Globalization;
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

        private SaveData saveData;
      
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

            settingsangle.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                SettingsAngle childwindow2 = new SettingsAngle();
                childwindow2.ShowInTaskbar = false;
                childwindow2.Owner = Application.Current.MainWindow;
                childwindow2.ShowDialog();

                FHKbox = childwindow2.checkHip;
                SHKbox = childwindow2.checkKnee;
                
            });
        
           

            this.saveData = new SaveData();

            
        }
        public string comport = null;
        public int durationtime = 0;
        public string filename = null;

        Thread heartrateThread;
        Thread plotAnglesThread;




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
            Application.Current.Shutdown();
        }

        private void restartbutton_Click(object sender, RoutedEventArgs e)
        {
            
//
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
                            this.DrawBonesAndJoints(skel, dc);
                            //Console.WriteLine("Innan calcvelocity");
                            this.CalculateVelocity(skel, dc);
                            this.CalculateAngles(skel, dc);
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

                if ((bool)FHKbox.IsChecked || (bool)SHKbox.IsChecked)
                {
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
                else
                {
                    MessageBox.Show("Du måste välja en vinkel!");
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


        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// 


        //------------------------------- Hastighetsberäkning -----------------------------------// 

        // Variabler för hastighet
        double deltaT = 0;
        double deltaX = 0;
        double stepVelocity = 0;

        // Listor för hastighet
        List<double> velXList = new List<double>();
        List<double> velHelpList = new List<double>();
        List<double> velocityList = new List<double>();
        List<double> velocityListSave = new List<double>();

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

            if (velocityList.Count == 15)
                {
                double maxValue = velocityList.Max();
                velHelpList.Add(maxValue);
                velocityList.Clear();
            }

            if (velHelpList.Count == 8)
                    {
                double maxValueHelp = velHelpList.Average();
                initVel.Text = Convert.ToString(Math.Floor(maxValueHelp * 3.6)) + " km/h"; //Här ska det läggas till round
                velocityListSave.Add(maxValueHelp);
                        velHelpList.Clear();
                    }

            if (velXList.Count > 600)
            {
                    velXList.RemoveAt(0);
                }
            //saveData.ExcelVelocityFunk(velocityListSave);
            }


        // -------------------------------------------------------------------------------------//
        //------------------------------- Vinkelberäkning --------------------------------------//
        // -------------------------------------------------------------------------------------//



        // Skapar listorna som behövs
        public List<double> vinklar_FHK = new List<double>();
        public List<double> vinklar_SHK = new List<double>();
        public List<double> tidsLista = new List<double>();
        public List<double> minimumlista_FHK = new List<double>();
        public List<double> minimumlistahelp_FHK = new List<double>();
        public List<double> minimumlista_SHK = new List<double>();
        public List<double> minimumlistahelp_SHK = new List<double>();
        public List<double> meanList_FHK = new List<double>();
        public List<double> meanList_SHK = new List<double>();
        public List<double> pulseList = new List<double>();

        // Skapar variablerna som behövs
        public double sampleToTime_FHK = 0;
        public double sampleToTime_SHK = 0;
        public double lagsta_varde_FHK = 0;
        public double lagsta_varde_SHK = 0;
        public int updateMatlab = 0;
        public double meanAngle_FHK;
        public double meanAngle_SHK;
        int i = 0;
        int k = 0;
        //Skapar vektorer
        


        private List<double> TEST = new List<double>();

        public List<List<double>> meanAngleFunchelp(List<double> minList1, List<double> minList2)
        {

        
            if (i == k)
            meanAngle_FHK = minList1.Sum() / (minList1.Count);
            meanList_FHK.Add(meanAngle_FHK);
            meanAngleFunc(minList1);
            meanAngleBlock_FHK.Text = Convert.ToString(Math.Ceiling(meanList_FHK.LastOrDefault()));

            if (i == k)
            meanAngle_SHK = minList2.Sum() / (minList2.Count);
            meanList_SHK.Add(meanAngle_SHK);
            meanAngleFunc(minList2);
            meanAngleBlock_SHK.Text = Convert.ToString(Math.Ceiling(meanList_SHK.LastOrDefault()));

            if (i == k)
            {
                i = 0;
            }

            List<List<double>> output2 = new List<List<double>>();
            output2.Add(meanList_FHK);
            output2.Add(meanList_SHK);

            saveData.ExcelFunkFHK(meanList_FHK);
            saveData.ExcelFunkSHK(meanList_SHK);

            return output2;
        }

        public List<List<double>> meanAngleFunchelpOnebox(List<double> minList1)
        {
            if (minList1 == minimumlista_FHK)
            {

                if (i == k)
                meanAngle_FHK = minList1.Sum() / (minList1.Count);
                meanList_FHK.Add(meanAngle_FHK);
                meanAngleFunc(minList1);
                meanAngleBlock_FHK.Text = Convert.ToString(Math.Ceiling(meanList_FHK.LastOrDefault()));

                if (i == k)
                {
                    i = 0;
                }

                List<List<double>> output2 = new List<List<double>>();
                output2.Add(meanList_FHK);

                saveData.ExcelFunkFHK(meanList_FHK);

                return output2;
            }
            else
            {

                if (i == k)
                meanAngle_SHK = minList1.Sum() / (minList1.Count);
                meanList_SHK.Add(meanAngle_SHK);
                meanAngleFunc(minList1);
                meanAngleBlock_SHK.Text = Convert.ToString(Math.Ceiling(meanList_SHK.LastOrDefault()));

                if (i == k)
                {
                    i = 0;
                }

                List<List<double>> output2 = new List<List<double>>();
                output2.Add(meanList_SHK);

            saveData.ExcelFunkSHK(meanList_SHK);

            return output2;
        }

        }


        // Beräknar vinklar beroende på checkboxar
        public void CalculateAngles(Skeleton skeleton, DrawingContext drawingcontext)
        {
           // plotAngles();

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
          /*  ++count1;
            if (count1 == 600)
            {
                count1 = 0;
            }*/
            //-----------------------------------------------// 

            // Om båda checkboxarna är ifyllda så slängs ett felmeddelande och boxarna töms
         /*   if ((bool)SHKbox.IsChecked && (bool)FHKbox.IsChecked)
            {
                felmeddelande.Text = "Det går endast att mäta en vinkel åt gången!";
                SHKbox.IsChecked = false;
                FHKbox.IsChecked = false;
            }
            */
            // Kollar om SHKcheckbox är ifylld
            if ((bool)SHKbox.IsChecked)
            {
                felmeddelande.Text = "";

                double HipKnee_Length = Math.Sqrt(Math.Pow(XHipleft - XKneeleft, 2) + Math.Pow(YHipleft - YKneeleft, 2));
                double HipShoulder_Length = Math.Sqrt(Math.Pow(XHipleft - XShoulderleft, 2) + Math.Pow(YHipleft - YShoulderleft, 2));
                double KneeShoulder_Length = Math.Sqrt(Math.Pow(XKneeleft - XShoulderleft, 2) + Math.Pow(YKneeleft - YShoulderleft, 2));

                //SHK - Cosinussatsen för vinkel Höft-knä-fot, avrundar till heltal
                double SHK_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(HipShoulder_Length, 2)
                    - Math.Pow(KneeShoulder_Length, 2)) / (2 * HipKnee_Length * HipShoulder_Length))) * (180 / Math.PI));

                if (Double.IsNaN(SHK_angle))
                {
                    double prevValSHKlist = vinklar_SHK[vinklar_SHK.Count - 1];
                    minimumlistahelp_SHK.Add(prevValSHKlist);
                    vinklar_SHK.Add(prevValSHKlist);
                    contAngle_SHK.Text = "HEJ MY LITTLE PONY!!! SAILOR MOON";

                }
                else
                {
                vinklar_SHK.Add(SHK_angle);
                minimumlistahelp_SHK.Add(SHK_angle);
                }

                contAngle_SHK.Text = Convert.ToString(i);
              //  contAngle_SHK.Text = Convert.ToString();
                sampleToTime_SHK = vinklar_SHK.Count;

                if (SHK_angle < 140)
                {
                    textTestdirektiv.Text = "Stand up straight!";
                    SystemSounds.Asterisk.Play();
                }
                else
                {
                    textTestdirektiv.Text = "";
                }
            }

            // Kollar om FHKcheckbox är ifylld
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

      

                if (Double.IsNaN(FHK_angle))
                {
                    double prevValFHKlist = vinklar_FHK[vinklar_FHK.Count - 1];
                    minimumlistahelp_FHK.Add(prevValFHKlist);
                    vinklar_FHK.Add(prevValFHKlist);

                }
                else
                {
                vinklar_FHK.Add(FHK_angle);
                minimumlistahelp_FHK.Add(FHK_angle);
                }

                contAngle_FHK.Text = Convert.ToString(meanList_FHK.Count);
                sampleToTime_FHK = vinklar_FHK.Count;

                if (FHK_angle < 90)
                {
                    textTestdirektiv.Text = "pull out your knees!";
                    SystemSounds.Asterisk.Play();
                }
                else
                { 
                    textTestdirektiv.Text = "";
                }
            }

            // Lägger till i tidslista beroende på vilken box som är vald

            if (((bool)FHKbox.IsChecked) && (bool)SHKbox.IsChecked)
            {
                tidsLista.Add(sampleToTime_FHK / 30);
            
                if (minimumlistahelp_FHK.Count > 60)
            {
                //Knävinkel
                lagsta_varde_FHK = minimumlistahelp_FHK.Min();
                minimumlista_FHK.Add(lagsta_varde_FHK);
                smallestAngle_FHK.Text = Convert.ToString(lagsta_varde_FHK);
                minimumlistahelp_FHK.Clear();
                //Höftvinkel
                lagsta_varde_SHK = minimumlistahelp_SHK.Min();
                minimumlista_SHK.Add(lagsta_varde_SHK);
                minimumlistahelp_SHK.Clear();
                smallestAngle_SHK.Text = Convert.ToString(lagsta_varde_SHK);
                meanAngleFunchelp(minimumlista_FHK, minimumlista_SHK);

                meanAngleFunc(meanList_FHK);
                meanAngleFunc(meanList_SHK);
                ++i;

                readPulseData();
                }
                else
                {
                    minimumlista_FHK.Add(lagsta_varde_FHK);
                    minimumlista_SHK.Add(lagsta_varde_SHK);
                }
            }
            
             if ((bool)FHKbox.IsChecked)
            {
                tidsLista.Add(sampleToTime_FHK / 30);

                if (minimumlistahelp_FHK.Count > 60)
                {
                    //Knävinkel
                    lagsta_varde_FHK = minimumlistahelp_FHK.Min();
                    minimumlista_FHK.Add(lagsta_varde_FHK);
                    smallestAngle_FHK.Text = Convert.ToString(lagsta_varde_FHK);
                    minimumlistahelp_FHK.Clear();
                    meanAngleFunchelpOnebox(minimumlista_FHK);
          
                    meanAngleFunc(meanList_FHK);
                    ++i;

                readPulseData();
            }
            else
            {
                minimumlista_FHK.Add(lagsta_varde_FHK);

                }
            }
            else if ((bool)SHKbox.IsChecked)
            {
                tidsLista.Add(sampleToTime_SHK / 30);

                if (minimumlistahelp_SHK.Count > 60)
                {
                    //Höftvinkel
                    lagsta_varde_SHK = minimumlistahelp_SHK.Min();
                    minimumlista_SHK.Add(lagsta_varde_SHK);
                    minimumlistahelp_SHK.Clear();
                    smallestAngle_SHK.Text = Convert.ToString(lagsta_varde_SHK);
                    meanAngleFunchelpOnebox(minimumlista_SHK);


                    meanAngleFunc(meanList_SHK);
                    ++i;

                    readPulseData();
            }
            else
            {

                minimumlista_SHK.Add(lagsta_varde_SHK);
            }
        }
        }


        // För att ta ut medelvinkel
        private void meanAngleFunc(List<double> minList)
        {
            minList.Sort();
            int listIndex = 0;
            while (listIndex < minList.Count - 1)
            {
                if (minList[listIndex] == minList[listIndex + 1])
                {
                    minList.RemoveAt(listIndex);
                }
                else
                {
                    ++listIndex;
                }
            }
        }
        


        
    // -------------------------------------------------------------------------------------//
    // -------------------------- Saker som ritar ------------------------------------------//
    // -------------------------------------------------------------------------------------//

    // Skickar allting till matlab och plottas sedan
    void printMatLab(List<double> list1, List<double> list2, List<double> list3)
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
                    
                    matlab.Feval("myfunc", 1, out result, list1.ToArray(), list2.ToArray(), list3.ToArray());
                }
                catch (System.Runtime.InteropServices.COMException)
                {

                }

            
        }


        // Skickar allting till matlab och plottas sedan
       public void printMatLab1(string funktionsnamn, string comport, int durationtime, string fileName)
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
                matlab.Feval(funktionsnamn, 0, out result, comport.ToString(), durationtime, fileName);
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

        private void FKHbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SHKbox_Checked(object sender, RoutedEventArgs e)
        {

        }


        private void display_heartrate_Click(object sender, RoutedEventArgs e)
        {


            if (comport == "" || durationtime == 0 || filename == "")
            {
                MessageBox.Show("Add heartrate settings");
            }
            else
            {
              
                heartrateThread = new Thread(() => printMatLab1("heartRateCalc", comport, durationtime, filename));
                heartrateThread.Start();
          
            }
        }
                


        private void display_angle_Click(object sender, RoutedEventArgs e)
        {
            CompositionTargetRendering();
            plotAnglesThread = new Thread(() => printMatLab(tidsLista, minimumlista_FHK, minimumlista_SHK));
            plotAnglesThread.Start();

           //  printMatLab(tidsLista, vinklar_FHK, vinklar_SHK);               
        }
        
        private void setting_Click(object sender, RoutedEventArgs e)
        {

        }



        private  void readPulseData()
        {
                try
                {
                var currentpath = Path.Combine(Directory.GetCurrentDirectory());
                String line = File.ReadLines(currentpath + @"\..\..\pulsdata1.txt").Last();
                double pulsTodec = Double.Parse(line, NumberStyles.Float,CultureInfo.InvariantCulture);

                pulseList.Add(Math.Ceiling(pulsTodec));

                pulstest.Text = Convert.ToString(Math.Ceiling(pulsTodec));  
  
            }

                catch (Exception e)
                {
                   Console.WriteLine("Error: " + e.Message);

                }

            saveData.ExcelPulseFunk(pulseList);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
           // SaveData win2 = new SaveData();
            saveData.Show();
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
                k = 5;
            }
            if (comboBox.SelectedIndex == 1)
            {
                k = 10;
            }
            if (comboBox.SelectedIndex == 2)
            {
                k = 30;
            }
            if (comboBox.SelectedIndex == 3)
            {
                k = 2;
            }
        }

        public void plotAngles()
        {
            if (vinklar_SHK.Count() > updateMatlab || vinklar_FHK.Count() > updateMatlab)
            {
                CompositionTargetRendering();
                plotAnglesThread = new Thread(() => printMatLab(tidsLista, vinklar_FHK, vinklar_SHK));
                plotAnglesThread.Start();
                updateMatlab = updateMatlab + 60;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("10 Sekunders Intervall");
            data.Add("20 Sekunders intervall");
            data.Add("60 Sekunders intervall");
            data.Add("2 Sekunders intervall");

            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;

       }
        
        private System.Windows.Forms.Timer timer1;
        private int counter; 
        private void startLoggingButton_Click(object sender, EventArgs e)
        {
            //meanList_SHK.Clear();
            //meanList_FHK.Clear();
            //pulseList.Clear();

            if (comboBox1.SelectedIndex == 0)
            {
                counter = 10;
            }
            if (comboBox1.SelectedIndex == 1)
            {
                counter = 30;
            }
            if (comboBox1.SelectedIndex == 2)
            {
                counter = 60;
            }
            if (comboBox1.SelectedIndex == 3)
            {
                counter = 300;
            }
            if (comboBox1.SelectedIndex == 4)
            {
                counter = 600;
            }
          
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();
            timerContent.Text = counter.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter == 0)
                timer1.Stop();
            timerContent.Text = counter.ToString();
        }


        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox1 = sender as ComboBox;

            // ... Set SelectedItem as Window Title.
            string BoxValue = comboBox1.SelectedItem as string;
            saveData.ExcelTestLength(comboBox1.SelectedIndex);
        }

        public int testLength;

        public void ComboBox1_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("10 Seconds Test ");
            data.Add("30 Seconds Test");
            data.Add("60 Seconds Test");
            data.Add("5 Minutes Test");
            data.Add("10 Minutes Test");

            // ... Get the ComboBox reference.
            var comboBox1 = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox1.ItemsSource = data;

            // ... Make the first item selected.
            comboBox1.SelectedIndex = 0;



        
        }
    }


    }

    