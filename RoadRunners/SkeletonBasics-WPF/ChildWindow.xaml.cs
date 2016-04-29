
using System.Windows;

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

        //Kollar så comport är ifylld
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
            MessageBox.Show(error_message);
        }
    }
}
