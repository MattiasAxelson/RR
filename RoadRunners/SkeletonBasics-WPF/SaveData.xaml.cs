using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Den här klassen skapar exceldokmumentet
    /// </summary>
    public partial class SaveData : System.Windows.Window
    {
        
        public SaveData()
        {
            InitializeComponent();
            // Skapar objektet mWindow med samma egenskaper som MainWindow men bara under den tiden som Save knappen trycks. 
            /* SaveButtonExcel.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
             {
                 MainWindow mWindow = new MainWindow();
             }
             );*/
        }

        // Definirerar de listor som behövs till funktionerna
        List<double> ExcelMeanListFHKhelp = new List<double>();
        List<double> ExcelMeanListSHKhelp = new List<double>();
        List<double> ExcelPulseListhelp = new List<double>();
        List<double> ExcelVelocityListHelp = new List<double>();
        
        // Definerar hjälpvariabler som behövs för vissa funktioner
        int Intervallhelp;
        int TestLengthHelp;
        public int testLength = 0;

        // Definerar funktioner som används för att hämta variabler i MainWindow
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

        public int ReturnTestLength()
        {
            if (TestLengthHelp == 0)
            {
                testLength = 10;
            }
            if (TestLengthHelp == 1)
            {
                testLength = 30;
            }
            if (TestLengthHelp == 2)
            {
                testLength = 60;
            }
            if (TestLengthHelp == 3)
            {
                testLength = 300;
            }
            if (TestLengthHelp == 4)
            {
                testLength = 600;
            }
            if (TestLengthHelp == 5)
            {
                testLength = 1800;
            }
            if (TestLengthHelp == 6)
            {
                testLength = 3600;
            }
            return testLength;
        }



        public void Save_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow mWindow = new MainWindow();

            // Skapar variabler för som sätts om beroende på vilket intervall och testlängd användaren väljer
            int Intervall = 0;
           

            // Hämtar aktuell tid samt namn och kommentar som användaren fört in
            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm tt");
            string Namn = NameInput.Text;
            string Comments = CommentsInput.Text;

            // Skapar Exceldokumentet
            Excel.Application app = new Excel.Application();
            app.Visible = true;
            app.WindowState = XlWindowState.xlMaximized;
            Workbook wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = wb.Worksheets[1];
            DateTime currentDate = DateTime.Now;
            Excel.Range aRange;
            aRange = ws.get_Range("A1", "M100");

            //Lägger in Rubriker, första intervallet, tid, namn och kommentarer i dokumentet
            ws.Range["A3"].Value = "'" + 0 + "-" + (Intervall);
            ws.Range["F1"].Value = "Namn: " + Namn;
            ws.Range["G1"].Value = "Comments: " + Comments;
            ws.Range["H1"].Value = "Date and Time: " + time;
            ws.Range["A2"].Value = "Intervall [sec]";
            ws.Range["B2"].Value = "HeartBeat";
            ws.Range["C2"].Value = "Angle Knee";
            ws.Range["D2"].Value = "Angle Hip"; 
            ws.Range["E2"].Value = "Velocity";

            // Kontrollerar vilket intervall som valts av användaren
            if (Intervallhelp == 0)
            {
                Intervall = 2;
            }
            if (Intervallhelp == 1)
            {
                Intervall = 10;
            }
            if (Intervallhelp == 2)
            {
                Intervall = 20;
            }
            if (Intervallhelp == 3)
            {
                Intervall = 60;
            }

            // Kontrollerar vilken testlängd som valts av användaren 
            testLength = ReturnTestLength();

            // Sätter duration som anger hur många rader som ska skrivas ut i excel
            int duration = (testLength / Intervall);

            //Skriver ut insamlade data i excel beroende på hur länge testet körs samt dess intervall
            for (int i = 0; i < duration; i++) 
            {
                ws.Range["A0" + (i + 3)].Value = "'" + (i * Intervall) + "-" + (i + 1) * Intervall;

                if (ExcelPulseListhelp != null && ExcelPulseListhelp.Count > i)
                {
                    ws.Range["B0" + (i + 3)].Value = (ExcelPulseListhelp[i]);
                }
                else
                {
                    ws.Range["B0" + (i + 3)].Value = "No Data";
                }

                if (ExcelMeanListFHKhelp != null && ExcelMeanListFHKhelp.Count > i)
                {
                    ws.Range["C0" + (i + 3)].Value = Math.Ceiling(ExcelMeanListFHKhelp[i]);
                }
                else
                {
                    ws.Range["C0" + (i + 3)].Value = "No Data";
                }
                if (ExcelMeanListSHKhelp != null && ExcelMeanListSHKhelp.Count > i)
                {
                    ws.Range["D0" + (i + 3)].Value = Math.Ceiling(ExcelMeanListSHKhelp[i]);
                }
                else
                {
                    ws.Range["D0" + (i + 3)].Value = "No Data";
                }
                if (ExcelVelocityListHelp != null && ExcelVelocityListHelp.Count > i)
                {
                  ws.Range["E0" + (i + 3)].Value = ExcelVelocityListHelp[i];
                }
            }

            // Gör så att cellerna anpassas till längden som matas in i dem
            aRange.Columns.AutoFit();
            aRange.EntireColumn.AutoFit();

            // Gör så att användaren får välja var testet ska sparas samt stänger fönstret
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                wb.SaveAs(saveFileDialog.FileName);
                this.Hide();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
