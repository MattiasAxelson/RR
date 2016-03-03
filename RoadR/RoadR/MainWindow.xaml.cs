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
using Microsoft.Kinect;

namespace RoadR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //oklart vilken man ska ha, men man kan testa båda
        Skeleton skeletons = new Skeleton();
        Skeleton[] skeleton2 = new Skeleton[6];

        public MainWindow()
        {
            InitializeComponent();
        }

   

        //Startar kinecten
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser_KinectSensorChanged);
        }

        //
        void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);
            KinectSensor newSensor = (KinectSensor)e.NewValue;


            //aktiverar streamsen aa
            newSensor.ColorStream.Enable();
            newSensor.SkeletonStream.Enable();
            newSensor.DepthStream.Enable();

            //
            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser.AppConflictOccurred();
            }
        }

        //stänger kameran då fönstret stängs
        private void Window_Closed(object sender, EventArgs e)
        {
            StopKinect(kinectSensorChooser.Kinect);
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();

            }
        }


        //kameratilt
        private void Kinect_angle_Click(object sender, RoutedEventArgs e)
        {
            if (kinectSensorChooser.Kinect.ElevationAngle != (int)slider.Value)
            {
                kinectSensorChooser.Kinect.ElevationAngle = (int)slider.Value;
            }
        }
        //kameratilt
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int n = (int)slider.Value;

            Degree.Content = n.ToString();
        }


        //test för koord, när man trycker på knappen bör koord uppdateras
        private void button_Click(object sender, RoutedEventArgs e)
        {

            Joint lKnee = skeletons.Joints[JointType.KneeLeft];
            Joint hand = skeletons.Joints[JointType.HandLeft];

            float yposs = lKnee.Position.Y;
            float xposs = lKnee.Position.X;


            textBlock.Text = yposs.ToString();
            Vinkel.Content = yposs.ToString();
        }
    }
}
