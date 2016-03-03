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

namespace WpfApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       

        KinectSensor _sensor1;
        Skeleton[] _skelett1 = new Skeleton[6];
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor1 = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (_sensor1 != null)
            {
                _sensor1.ColorStream.Enable();
                _sensor1.SkeletonStream.Enable();

                _sensor1.AllFramesReady += Sensor_AllFramesReady;

                _sensor1.Start();
            }
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
             
            }
            using (var frame = e.OpenSkeletonFrame())
            {

            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
