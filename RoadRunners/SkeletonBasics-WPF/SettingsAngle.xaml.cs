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
    /// Interaction logic for SettingsAngle.xaml
    /// </summary>
    public partial class SettingsAngle : Window
    {
        public SettingsAngle()
        {
            InitializeComponent();
        }

        private void anglehelp_Click(object sender, RoutedEventArgs e)
        {
            string error_message = "Please select which angle you want to observe";
            System.Windows.MessageBox.Show(error_message);
        }

        private void checkKnee_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void checkHip_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
