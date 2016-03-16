using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Read_textfile_aquav2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
        int counter = 0;
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Per\Dropbox\Arbeten\TBMT41\2016-03-09.Testpulser\13_38_20\Simonpuls132tid60.txt");
            List<int> list = new List<int>();
            while ((line = file.ReadLine()) != null)
            {
                listBox1.Items.Add(line);
                list.Add(int.Parse(line));
                counter++;
            }
            int[] arr = list.ToArray();
            double beats = 0;
            int[] arr_time = null;
            int hz = 100;
            double time = arr.Length / hz;

            int max = arr.Max();
            int min = arr.Min();

            while (min < 10)
            {
                arr = arr.Where(val => val != min).ToArray();
                min = arr.Min();
            }

            int diff = max - min;


            int topp = 5000;

            for (int i = 2; i < arr.Length - 2; i++)
            {
                if (arr[i + 2] + topp < arr[i] && arr[i - 2] + topp < arr[i])
                {
                    if (arr_time == null)
                    {
                        arr_time = new int[i];
                        beats = beats + 1;
                    }
                    else if (arr_time.Last() + 250 < i)
                    {
                        arr_time = new int[i];
                        beats = beats + 1;
                    }
                }
            }

            double BPM = beats / (time / 60);

            listBox2.Items.Add(topp);
            listBox3.Items.Add(beats);
            listBox4.Items.Add(time);
            listBox5.Items.Add(BPM);
            listBox6.Items.Add(max);
            listBox7.Items.Add(min);
            listBox8.Items.Add(diff);
            Array.Sort(arr);
            foreach (int item in arr)
            {

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
