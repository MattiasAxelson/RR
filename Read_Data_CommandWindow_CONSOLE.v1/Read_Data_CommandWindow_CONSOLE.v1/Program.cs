using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Read_Data_CommandWindow_CONSOLE.v1
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "Hej Per";
         
            Console.WriteLine(text);

           string data = Console.ReadLine();

          

            System.IO.File.WriteAllText(@"C:\Users\Per\Dropbox\Arbeten\TBMT41\C#\test.txt", text);

            //  if (data != null)
            // {
            //   listBox1.Items.Add(data);
            // }
        }
    }
}
