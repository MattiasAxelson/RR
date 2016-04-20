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
        int Intervallhelp;
        int TestLengthHelp;

        List<double> ExcelPulseListhelp = new List<double>();
        List<double> ExcelVelocityListHelp = new List<double>();

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

        public void ExcelVelocityFunk(List<double> tempVelocityList)
        {
            ExcelVelocityListHelp = tempVelocityList;
        }

        public void ExcelTestLength(int tempTestLength)
        {
            TestLengthHelp = tempTestLength;
        }





        public void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mWindow = new MainWindow();
           // double[] MeanAngleFHK = mWindow.meanArray_FHK;

            int Intervall = 666;
            int testLength = 0;

            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm tt");
            string Namn = NameInput.Text;
            string Comments = CommentsInput.Text;



            testArray.Text = Convert.ToString(Intervall);

            List<double> ExcelMeanListFHK = ExcelMeanListFHKhelp;
            List<double> ExcelMeanListSHK = ExcelMeanListSHKhelp;
            List<double> ExcelPulseList = ExcelPulseListhelp;
            List<double> ExcelVelocityList = ExcelVelocityListHelp; 

            int ExcelIntervall = Intervallhelp;
            int DurationTime = TestLengthHelp;

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
            ws.Range["F1"].Value = "Namn: " + Namn;
            ws.Range["G1"].Value = "Comments: " + Comments;
            ws.Range["H1"].Value = "Date and Time: " + time;
            ws.Range["A2"].Value = "Intervall [sec]";
            ws.Range["B2"].Value = "HeartBeat";
            ws.Range["C2"].Value = "Angle Knee";
            ws.Range["D2"].Value = "Angle Hip"; 
            ws.Range["E2"].Value = "Velocity";

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


            // Kollar vilken testlängd som valts i combobox1 
            if (DurationTime == 0)
            {
                testLength = 10;
            }
            if (DurationTime == 1)
            {
                testLength = 30;
            }
            if (DurationTime == 2)
            {
                testLength = 60;
            }
            if (DurationTime == 3)
            {
                testLength = 300;
            }
            if (DurationTime == 4)
            {
                testLength = 600;
            }


            int duration = (testLength / Intervall);



            testArray.Text = duration.ToString();
            testArray_Copy.Text = testLength.ToString();
            testArray_Copy1.Text = testLength.ToString();

            ws.Range["F3"].Value = duration;
            ws.Range["F4"].Value = testLength;
            ws.Range["F5"].Value = Intervall;

            for (int i = 0; i < duration; i++) 
            {

                ws.Range["A0" + (i + 3)].Value = "'" + (i * Intervall) + "-" + (i + 1) * Intervall;

                if (ExcelPulseList != null)
                {
                    ws.Range["B0" + (i + 3)].Value = ((ExcelPulseList.Skip(i * Intervall).Take(Intervall).Sum())/Intervall); 
                }

                if (ExcelMeanListFHK != null && ExcelMeanListFHK[i] >= 0 && ExcelMeanListFHK[i] < 200)
                {
                    ws.Range["C0" + (i + 3)].Value = Math.Ceiling(ExcelMeanListFHK[i]);
                }
                else
                {
                    ws.Range["C0" + (i + 3)].Value = "ERROR";
                }
                 if (ExcelMeanListSHK != null && ExcelMeanListSHK[i] > 0 && ExcelMeanListSHK[i] < 200)
                {
                    ws.Range["D0" + (i + 3)].Value = Math.Ceiling(ExcelMeanListSHK[i]);
                }
                else
                {
                    ws.Range["D0" + (i + 3)].Value = "ERROR";
                }
                
                 if (ExcelVelocityList != null)
                 {
                    ws.Range["E0" + (i + 3)].Value = ExcelVelocityList[i];

                 }
            }



            aRange.Columns.AutoFit();
            aRange.EntireColumn.AutoFit();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                wb.SaveAs(saveFileDialog.FileName);

            this.Hide();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
