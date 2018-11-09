using System;
using System.Collections.Generic;
using System.IO;

namespace CompareCsv
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CompareCSVFiles compareCSV = new CompareCSVFiles();

            compareCSV.Start();
        }
    }
}
