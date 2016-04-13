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

namespace ExcelData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
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

            int[] numbers = new int[] {1, 2, 3, 4, 5, 6, 7, 8};
            for (int i = 1; i <= numbers.Length; i++)
                ws.Range["E" + i].Value = numbers[i - 1];

            ws.Range["A1:A3"].Value = "Who is number one? :)";
            ws.Range["A4"].Value = "vitoshacademy.com";
            ws.Range["A5"].Value = currentDate;
            ws.Range["B6"].Value = "Tommorow's date is: =>";
            ws.Range["C6"].FormulaLocal = "= A5 + 1";
            ws.Range["A7"].FormulaLocal = "=SUM(D1:D10)";
            for (int i = 1; i <= 10; i++)
                ws.Range["D" + i].Value = i * 2;

            wb.SaveAs("C:\\Users\\Per\\Desktop\\vitoshacademy5.xlsx");
        
        }
    }
}
