using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication4
{
    class Program
    {
        static void Main()
        {
            R();
        }

        static void R()
        {
            // 1.

            //  int[] arr_time = null;

            using (BinaryReader b = new BinaryReader(File.Open(@"C:\Users\Per\Dropbox\Arbeten\TBMT41\Pulssensorn\Aqua 2.5.0.19b\Datafiler\2015-04-09\14_26_57\test1.bin", FileMode.Open)))
            {
                // 2.
                // Position and length variables.
                int pos = 0;
                // 2A.
                // Use BaseStream.
                int length = (int)b.BaseStream.Length;
                while (pos < length)
                {
                    // 3.
                    // Read integer.
                    int v = b.ReadUInt16();

                    Console.WriteLine(v);
                    //   arr_time = new int[v];

                    // 4.
                    // Advance our position variable.
                    pos += sizeof(int);
                }
            }
        }
    }

    internal class int16_t
    {
    }
}
