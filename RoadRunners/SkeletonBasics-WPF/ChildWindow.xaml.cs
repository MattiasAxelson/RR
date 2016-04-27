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

        public int durationtime;

        //kollar om det INTE bara är siffror
        bool onlyDigits(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return true;

            }
            return false;
        }

        //kollar om det INTE bara är bokstäver
        bool onlyLetters(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsLetter(c))
                    return true;
            }
            return false;
        }


        void startHeartRateCalc_Click(object sender, RoutedEventArgs e)
        {
            comport = comportContent.Text;   

            if(onlyDigits(comport) || comport == "")
            {
                errorText.Text = "Add your comport, \n" +
                    "only digits allowed";
            }

            else
            {
                this.Close();
            }
            


        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            string error_message = "1. Add the number of your comport \n";
            System.Windows.MessageBox.Show(error_message);
        }
    }
}
