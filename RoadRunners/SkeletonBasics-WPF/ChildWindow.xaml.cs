using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for ChildWindow.xaml
    /// </summary>
    public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
        }

        public string comport;
        public string convdurationtime;
        public string filname;

        private void startHeartRateCalc_Click(object sender, RoutedEventArgs e)
        {
            comport = comportContent.Text;
            convdurationtime = durationContent.Text;
            filname = filenameContent.Text;
            int durationtime = int.Parse(convdurationtime);
        }
    }
}
