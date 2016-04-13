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
     public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
        }

        public string comport;
        public string convdurationtime;
        public string fileName;
        public int durationtime;
        
        void startHeartRateCalc_Click(object sender, RoutedEventArgs e)
        {
            comport = comportContent.Text;
            convdurationtime = durationContent.Text;
            fileName = filenameContent.Text;
            durationtime = int.Parse(convdurationtime);

            if (comport == "0" || durationtime == 0 || fileName == "" || durationContent.Text == "")
            {
                errorText.Text = "Du måste fylla i alla fälten korrekt.";
            }
            else
            {
                this.Close();
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            string error_message = "1. Fyll i comport genom att ange numret på den \n" +
                                   "comport som pulssensorn är ansluten till.\n" +
                                   "2. Ange under hur lång tid (i sekunder) du vill mäta din puls.\n" +
                                   "3. Fyll i ett filnamn endast innehållande bokstäver \n" +
                                   "som du vill spara filerna till.";
            System.Windows.MessageBox.Show(error_message);
        }
    }
}
