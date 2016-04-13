
using System.Windows;



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
        public string tid;
        public string filename;

       

        private void heartRateButton_Click(object sender, RoutedEventArgs e)
        {

            comport = comportTextbox.Text;
            tid = testtimeTextBox.Text;
            filename = fileNametextBox.Text;
            int tiden = int.Parse(tid);
           
            
            
        }

        


    }
}
