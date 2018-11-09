using CompareCsv.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompareCsv
{
    public class CompareCSVFiles
    {
        private ISettings m_settings = LoadSettings();
        private double m_minimalSmallNumber = 0.0000001;

        static ISettings LoadSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("csv_settings.json")
                .AddJsonFile("csv_settings.developer.json", optional: true)
                .AddEnvironmentVariables("csv_");
            var config = builder.Build();
            var settings = config.Get<Settings>();
            return settings;
        }

        public void Start()
        {
            string FirstFileNameFullPath = Path.GetFullPath(Path.Combine(m_settings.BaseDir, m_settings.FirstFileName));
            string SecondFileNameFullPath = Path.GetFullPath(Path.Combine(m_settings.BaseDir, m_settings.SecondFileName));
            List<string[]> firstCSVFile;
            List<string[]> secondCSVFile;
            
            (firstCSVFile, secondCSVFile) = CSVToList(FirstFileNameFullPath, SecondFileNameFullPath);
            StringBuilder stringBuilder = CompareCsv(firstCSVFile, secondCSVFile);
            WriteTxtFile(stringBuilder);

            if (m_settings.IsDeleteFiles)
                DeleteOriginFiles(FirstFileNameFullPath, SecondFileNameFullPath);
        }

        private (List<string[]> firstCSVFile, List<string[]> secondCSVFile) CSVToList(string firstFileNamePath, string secondFileNamePath)
        {
            var csv1 = new List<string[]>();
            var csv2 = new List<string[]>();
            var linesFromFisrtFile = File.ReadAllLines(firstFileNamePath);
            var linesFromSecondFile = File.ReadAllLines(secondFileNamePath);

            for (int i = 0; i < linesFromFisrtFile.Length; i++)
            {
                csv1.Add(linesFromFisrtFile[i].Split(','));
                csv2.Add(linesFromSecondFile[i].Split(','));
            }

            return (csv1, csv2);
        }

        private StringBuilder CompareCsv(List<string[]> firstCSVFile, List<string[]> secondCSVFile)
        {
            StringBuilder sb = new StringBuilder();
            string msg = string.Empty;
            double diff = 9999999999;
            double sub1 = 0;
            double sub2 = 0;
            string IsCrucial = "Not Crucial";
            
            for (int i = 0, j = 0; i < firstCSVFile[i].Length + 1 && i < firstCSVFile.Count; j++)
            {
                if (j != firstCSVFile[i].Length && firstCSVFile[i][j] != secondCSVFile[i][j])
                {
                    if (m_settings.IgnoreWindData && (string.Compare((firstCSVFile[0][j]), " Wind Speed") == 0 
                        || string.Compare((firstCSVFile[0][j]), "Wind Direction") == 0
                        || string.Compare((firstCSVFile[0][j]), " Wind Direction") == 0))
                    {
                        msg = "Wind Data diff was ignored";
                        continue;
                    }

                    try
                    {
                        sub1 = firstCSVFile[i][j].Contains('"') 
                            ? double.Parse(firstCSVFile[i][j].Split('"').Skip(1).Take(1).FirstOrDefault())
                            : double.Parse(firstCSVFile[i][j]);

                        sub2 = secondCSVFile[i][j].Contains('"')
                            ? double.Parse(secondCSVFile[i][j].Split('"').Skip(1).Take(1).FirstOrDefault())
                            : double.Parse(secondCSVFile[i][j]);

                        diff = Math.Abs(sub1) - Math.Abs(sub2);

                        if (diff != default(double) && diff.ToString().Split('.').Skip(1).FirstOrDefault().Count() > 6)
                        {
                            IsCrucial = Math.Abs(diff) > m_minimalSmallNumber ? "CRUCIAL" : "Not Crucial";
                        }
                    }
                    finally
                    {
                        sb.AppendLine($"Column Name is: {firstCSVFile[0][j]}, file name is: {m_settings.FirstFileName} value is: {firstCSVFile[i][j]} file name is:{m_settings.SecondFileName} value is: {secondCSVFile[i][j]} and diff is: {diff}. The DIFF is {IsCrucial ?? IsCrucial: ''}");

						if (IsCrucial == "CRUCIAL")
							Log.Error($"Column Name is: {firstCSVFile[0][j]}, file name is: {m_settings.FirstFileName} value is: {firstCSVFile[i][j]} file name is:{m_settings.SecondFileName} value is: {secondCSVFile[i][j]} and diff is: {diff}. The DIFF is {IsCrucial ?? IsCrucial: ''}");
						else
							Log.Information($"Column Name is: {firstCSVFile[0][j]}, file name is: {m_settings.FirstFileName} value is: {firstCSVFile[i][j]} file name is:{m_settings.SecondFileName} value is: {secondCSVFile[i][j]} and diff is: {diff}. The DIFF is {IsCrucial ?? IsCrucial: ''}");
                    }
                }

                if (j == firstCSVFile[0].Length)
                {
                    i++;
                    j = 0;
                }
            }

            sb.AppendLine(msg);
            return sb;
        }

        private void WriteTxtFile(StringBuilder sb)
        {
            var newFileNameWithDiff = Path.Combine(m_settings.BaseDir, $"diff_{DateTime.Now.ToUniversalTime().ToString("ddMMyyyy_HHmmss")}.txt");
            string sbstring = sb.ToString();

            // Create the file.
            using (FileStream fs = File.Create(newFileNameWithDiff))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(sbstring);

                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }

        private void DeleteOriginFiles(string firstFileNameFullPath, string secondFileNameFullPath)
        {
            File.Delete(firstFileNameFullPath);
            File.Delete(secondFileNameFullPath);
        }
    }
}
