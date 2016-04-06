﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media.Imaging;
   

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



        //Skapar listor till vinklarna
        // kan nog sätta alla till private

            // lagrar vinklarna i en lista
        public List<double> vinklar = new List<double>();
        public List<double> tidsLista = new List<double>();
        public List<double> minimumlista = new List<double>();
        public List<double> minimumlistahelp = new List<double>();
        //public List<double> meanAngleList = new List<double>();




        public double helprefresh = 30;
        public double sampleToTime = 0;
        public double lagsta_varde;
        public int updateMatlab = 0;
        public double meanAngle = 170;
     
        
        // Create the MATLAB instance 
        MLApp.MLApp matlab = new MLApp.MLApp();


        private void stop_Button_Click(object sender, RoutedEventArgs e)
        {
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

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// 


        //VÅR VARIANT

        void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            /*if (checkBox1.IsChecked.Equals(true))
            {
                my_angle(skel, XShoulder, YShoulder, XHip, YHip, XFoot, YFoot);
            }*/
        }

        void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            /*if (checkBox2.IsChecked.Equals(true))
            {
                my_angle(skel, XHip, YHip, XKnee, YKnee, XFoot, YFoot);
            }*/
        }

        void my_angle(Skeleton skeleton, double bodypart1X, double bodypart1Y, double bodypart2X, double bodypart2Y, double bodypart3X, double bodypart3Y)
        {
            double bodypart12_Length = Math.Sqrt(Math.Pow(bodypart1X - bodypart2X, 2) + Math.Pow(bodypart1Y - bodypart2Y, 2));
            double bodypart13_Length = Math.Sqrt(Math.Pow(bodypart1X - bodypart3X, 2) + Math.Pow(bodypart1Y - bodypart3Y, 2));
            double bodypart23_Length = Math.Sqrt(Math.Pow(bodypart2X - bodypart3X, 2) + Math.Pow(bodypart2Y - bodypart3Y, 2));

            //cosinussatsen för vinkel, avrundar till heltal
            double the_angle = Math.Ceiling((Math.Acos((Math.Pow(bodypart12_Length, 2) + Math.Pow(bodypart23_Length, 2)
                - Math.Pow(bodypart13_Length, 2)) / (2 * bodypart12_Length * bodypart23_Length))) * (180 / Math.PI));

            return the_angle;
        
        }


        void calculate_angle(Skeleton skeleton)
        {

            //joints
            Joint footLeft = skeleton.Joints[JointType.FootLeft];
            Joint kneeLeft = skeleton.Joints[JointType.KneeLeft];
            Joint hipLeft = skeleton.Joints[JointType.HipLeft];
            Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];

            //Vinkel
            float XFoot;
            float YFoot;
            float XKnee;
            float YKnee;
            float XHip;
            float YHip;
            float XShoulder;
            float YShoulder;


            XFoot = footLeft.Position.X;
            YFoot = footLeft.Position.Y;
            XKnee = kneeLeft.Position.X;
            YKnee = kneeLeft.Position.Y;
            XHip = hipLeft.Position.X;
            YHip = hipLeft.Position.Y;
            XShoulder = shoulderLeft.Position.X;
            YShoulder = shoulderLeft.Position.Y;

            if (checkBox1.IsChecked.Equals(true))
            {
                my_angle(skeleton, XShoulder, YShoulder, XHip, YHip, XFoot, YFoot);
            }

            if (checkBox2.IsChecked.Equals(true))
            {
                my_angle(skeleton, XHip, YHip, XKnee, YKnee, XFoot, YFoot);
            }


            //Adderar vinkel till listan
            vinklar.Add(the_angle);
            sampleToTime = vinklar.Count;
            tidsLista.Add(sampleToTime / 30);
            minimumlistahelp.Add(the_angle);

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


        }




        //DERAS VARIANT

        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {

           
            //joints
            Joint footLeft = skeleton.Joints[JointType.FootLeft];
            Joint kneeLeft = skeleton.Joints[JointType.KneeLeft];
            Joint hipLeft = skeleton.Joints[JointType.HipLeft];
            Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];

            //Vinkel
            float XFootleft;
            float YFootleft;
            float XKneeleft;
            float YKneeleft;
            float XHipleft;
            float YHipleft;
            float XShoulderleft;
            float YShoulderleft;
           

            XFootleft = footLeft.Position.X;
            YFootleft = footLeft.Position.Y;
            XKneeleft = kneeLeft.Position.X;
            YKneeleft = kneeLeft.Position.Y;
            XHipleft = hipLeft.Position.X;
            YHipleft = hipLeft.Position.Y;
            XShoulderleft = shoulderLeft.Position.X;
            YShoulderleft = shoulderLeft.Position.Y;



            //vektorlängder
            double HipKnee_Length = Math.Sqrt(Math.Pow(XHipleft - XKneeleft, 2) + Math.Pow(YHipleft - YKneeleft, 2));
            double HipFoot_Length = Math.Sqrt(Math.Pow(XHipleft - XFootleft, 2) + Math.Pow(YHipleft - YFootleft, 2));
            double KneeFoot_Length = Math.Sqrt(Math.Pow(XKneeleft - XFootleft, 2) + Math.Pow(YKneeleft - YFootleft, 2));

            //cosinussatsen för vinkel Höft-knä-fot, avrundar till heltal
            double HKF_angle = Math.Ceiling((Math.Acos((Math.Pow(HipKnee_Length, 2) + Math.Pow(KneeFoot_Length, 2)
                - Math.Pow(HipFoot_Length, 2)) / (2 * HipKnee_Length * KneeFoot_Length))) * (180 / Math.PI));
            


          
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
                        matlab.Feval("myfunc", 1, out result, tidsLista.ToArray(), vinklar.ToArray(), minimumlista.ToArray());

                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {

                    }
                    updateMatlab = updateMatlab + 30;
                }
            

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
       


        // för att ändra tilten på kinecten
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


    }
}