﻿using System;
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

            using (BinaryReader b = new BinaryReader(File.Open(@"C:\Users\Per\Source\Repos\RR\ConsoleApplication5\Testfiler\20simon60puls.bin", FileMode.Open)))
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
                    int v = b.ReadInt16();
                    Console.WriteLine(v);
                    //   arr_time = new int[v];

                    // 4.
                    // Advance our position variable.
                    pos += sizeof(int);
                }
            }
        }
    }

}
