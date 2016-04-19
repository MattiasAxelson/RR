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
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for SaveData.xaml
    /// </summary>
    public partial class SaveData : System.Windows.Window
    {


        public SaveData()
        {

            InitializeComponent();

            // Skapar objektet mWindow med samma egenskaper som MainWindow men bara under den tiden som Save knappen trycks. 
            SaveButtonExcel.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                MainWindow mWindow = new MainWindow();
                //mWindow.ShowInTaskbar = false;
                //mWindow.Owner = Application.Current.SaveData;
                //mWindow.ShowDialog();
                // int [] PulseTRY = mWindow.HB10secTRY;

            });
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        List<double> ExcelMeanListFHKhelp = new List<double>();
        List<double> ExcelMeanListSHKhelp = new List<double>();
        int Intervallhelp = new int();
        List<double> ExcelPulseListhelp = new List<double>();

        public void ExcelFunkFHK(List<double> templistFHK)
        {
            ExcelMeanListFHKhelp = templistFHK;
        }
        public void ExcelFunkSHK(List<double> templistSHK)
        {
            ExcelMeanListSHKhelp = templistSHK;
        }

        public void ExcelFunkIntervall(int tempIntervall)
        {
            Intervallhelp = tempIntervall;
        }
        public void ExcelPulseFunk(List<double> tempPulseList)
        {
            ExcelPulseListhelp = tempPulseList;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mWindow = new MainWindow();
           // double[] MeanAngleFHK = mWindow.meanArray_FHK;

            int Intervall = 666;


            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm tt");
            string Namn = NameInput.Text;
            string Comments = CommentsInput.Text;



            testArray.Text = Convert.ToString(Intervall);

            List<double> ExcelMeanListFHK = ExcelMeanListFHKhelp;
            List<double> ExcelMeanListSHK = ExcelMeanListSHKhelp;
            List<double> ExcelPulseList = ExcelPulseListhelp;

            int ExcelIntervall = Intervallhelp;

            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;
            app.WindowState = XlWindowState.xlMaximized;
            Workbook wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = wb.Worksheets[1];
            Excel.Range aRange;
            DateTime currentDate = DateTime.Now;

        
            // Set the range to fill.
            aRange = ws.get_Range("A1", "M100");

            ws.Range["A3"].Value = "'" + 0 + "-" + (Intervall);
            ws.Range["E1"].Value = "Namn: " + Namn;
            ws.Range["F1"].Value = "Comments: " + Comments;
            ws.Range["G1"].Value = "Date and Time: " + time;
            ws.Range["A2"].Value = "Intervall [sec]";
            ws.Range["B2"].Value = "HeartBeat";
            ws.Range["C2"].Value = "Angle Hip";
            ws.Range["D2"].Value = "Angle Knee";

            if (ExcelIntervall == 0)
            {
                Intervall = 10;
            }
            if (ExcelIntervall == 1)
            {
                Intervall = 20;
            }
            if (ExcelIntervall == 2)
            {
                Intervall = 60;
            }
            if (ExcelIntervall == 3)
            {
                Intervall = 2;
            }

            



            // for (int i = 1; i < ExcelMeanListFHK.Count; i++)

            for (int i = 1; i < ExcelMeanListSHK.Count; i++) 
            {

                ws.Range["A0" + (i + 3)].Value = "'" + (i * Intervall) + "-" + (i + 1) * Intervall;

                 if (ExcelPulseList != null)
                    {
                    ws.Range["B0" + (i + 2)].Value = ((ExcelPulseList.Skip(i * Intervall).Take(Intervall).Sum())/Intervall); 
                    }


                if (ExcelMeanListFHK != null)
                {
                    ws.Range["C0" + (i + 2)].Value = ExcelMeanListFHK[i];
                }

                if (ExcelMeanListSHK != null)
                {
                    ws.Range["D0" + (i + 2)].Value = ExcelMeanListSHK[i];
                }
            }



            aRange.Columns.AutoFit();
            aRange.EntireColumn.AutoFit();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                wb.SaveAs(saveFileDialog.FileName);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
