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

namespace WpfApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if(checkBox.IsChecked.Equals(true))
            {
                Console.WriteLine("Den första är i kryssad");
           
            }

        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            if(checkBox1.IsChecked.Equals(true))
            {
                Console.WriteLine("Den andra är ikryssad");
            }
        }
    }
}
