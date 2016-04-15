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
using Microsoft.Office.Interop.Excel;
using SystemWindow = System.Windows.Window;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : SystemWindow
    {
        public DatabaseWindow()
        {
            InitializeComponent();
        }

              private void button_Click(object sender, RoutedEventArgs e)
            {
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;
            app.WindowState = XlWindowState.xlMaximized;

            Workbook wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = wb.Worksheets[1];
            DateTime currentDate = DateTime.Now;

            int[] HB10sec = new int[] { 1, 2, 31, 6, 5, 3213, 7, 8 };
            int[] Knee10sec = new int[] { 1, 2, 31, 666, 5, 3213, 7, 8 };
            int[] Hip10sec = new int[] { 1, 2, 31, 3213123, 5, 3213, 7, 8 };

            for (int i = 1; i <= HB10sec.Length; i++)
            {
                ws.Range["A0" + (i + 1)].Value = i * 10;
                ws.Range["B0" + (i + 1)].Value = (i + 1) * 10;
                ws.Range["C0" + (i + 1)].Value = HB10sec[i - 1];
                ws.Range["D0" + (i + 1)].Value = Knee10sec[i - 1];
                ws.Range["E0" + (i + 1)].Value = Hip10sec[i - 1];

            }
            ws.Range["A1"].Value = "Start [sec]";
            ws.Range["B1"].Value = "End [sec]";
            ws.Range["C1"].Value = "HB";
            ws.Range["D1"].Value = "Angle Hip";
            ws.Range["E1"].Value = "Angle Knee";

            wb.SaveAs("C:\\Users\\Simon\\Desktop\\Testdokumentering.xlsx");
        }
    }
}