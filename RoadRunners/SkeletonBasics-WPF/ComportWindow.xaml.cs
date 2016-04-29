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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ComportWindow : Window
    {
        public ComportWindow()
        {
            InitializeComponent();
        }
        public bool comportYesWasclicked = false;
        public bool comportNoWasclicked = false;

        private void comportYesButton_Click(object sender, RoutedEventArgs e)
        {
            comportYesWasclicked = true;
            Hide();
         
        }

        private void comportNoButton_Click(object sender, RoutedEventArgs e)
        {
            comportNoWasclicked = true;
 
        
            Hide();
              
        }
    }
}
