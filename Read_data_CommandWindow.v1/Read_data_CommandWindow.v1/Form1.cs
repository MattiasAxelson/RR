using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Read_data_CommandWindow.v1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Hej Per");
            string data;
            data = Console.ReadLine();

            if (data != null)
            {
                listBox1.Items.Add(data);
            }
            
        }
    }
}

namespace Read_Data_CommandWindow_CONSOLE.v1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hej Per");
            string data;
            data = Console.ReadLine();

            //  if (data != null)
            // {
            //   listBox1.Items.Add(data);
            // }
        }
    }
}