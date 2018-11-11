using CompareCsv.Services;
using Microsoft.Extensions.Configuration;
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
        private StringBuilder m_stringBuilder = new StringBuilder();


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
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            string FirstFileNameFullPath = Path.GetFullPath(Path.Combine(m_settings.BaseDir, m_settings.FirstFileName));
            string SecondFileNameFullPath = Path.GetFullPath(Path.Combine(m_settings.BaseDir, m_settings.SecondFileName));
            List<string[]> firstCSVFile;
            List<string[]> secondCSVFile;
            
            (firstCSVFile, secondCSVFile) = CSVToList(FirstFileNameFullPath, SecondFileNameFullPath);
            StringBuilder stringBuilder = CompareCsv(firstCSVFile, secondCSVFile);
            WriteTxtFile(stringBuilder);

            if (m_settings.IsDeleteFiles)
                DeleteOriginFiles(FirstFileNameFullPath, SecondFileNameFullPath);

            Console.WriteLine("Press Any Key To Continue");
            Console.ReadLine();
        }

        private (List<string[]> firstCSVFile, List<string[]> secondCSVFile) CSVToList(string firstFileNamePath, string secondFileNamePath)
        {
            var csv1 = new List<string[]>();
            var csv2 = new List<string[]>();
            var linesFromFisrtFile = File.ReadAllLines(firstFileNamePath);
            var linesFromSecondFile = File.ReadAllLines(secondFileNamePath);
            int length = 0;

            if (linesFromFisrtFile.Length < linesFromSecondFile.Length)
            {
                length = linesFromFisrtFile.Length;
                m_stringBuilder.AppendLine($"{m_settings.SecondFileName} is larger than {m_settings.FirstFileName}");
                Log.Information($"{m_settings.SecondFileName} is larger than {m_settings.FirstFileName}");
            }
            else if (linesFromFisrtFile.Length > linesFromSecondFile.Length)
            {
                length = linesFromSecondFile.Length;
                m_stringBuilder.AppendLine($"{m_settings.FirstFileName} is larger than {m_settings.SecondFileName}");
                Log.Information($"{m_settings.FirstFileName} is larger than {m_settings.SecondFileName}");
            }
            else
            {
                length = linesFromFisrtFile.Length;
            }

            for (int i = 0; i < length; i++)
            {
                csv1.Add(linesFromFisrtFile[i].Split(','));
                csv2.Add(linesFromSecondFile[i].Split(','));
            }

            return (csv1, csv2);
        }

        private StringBuilder CompareCsv(List<string[]> firstCSVFile, List<string[]> secondCSVFile)
        {
            string msg = string.Empty;
            double diff = 9999999999;
            double sub1 = 0;
            double sub2 = 0;
            string IsCrucial = "Not Crucial";

            for (int i = 0, j = 0; i < firstCSVFile.Count; j++)
            {
                if (j != firstCSVFile[i].Length && string.Compare(firstCSVFile[i][j], secondCSVFile[i][j]) != 0)
                {
                    if (m_settings.IgnoreWindData && (string.Compare((firstCSVFile[0][j]), " Wind Speed") == 0 
                        || string.Compare((firstCSVFile[0][j]), "Wind Direction") == 0
                        || string.Compare((firstCSVFile[0][j]), " Wind Direction") == 0))
                    {
                        msg = "Wind Data diff was ignored";

						if (j == firstCSVFile[0].Length - 1)
						{
							i++;
							j = 0;
						}
						continue;
                    }

                    try
                    {
						if ((!string.IsNullOrWhiteSpace(firstCSVFile[i][j]) && !string.IsNullOrEmpty(firstCSVFile[i][j]) && string.Compare("", firstCSVFile[i][j], StringComparison.InvariantCultureIgnoreCase) != 0)
							&& !string.IsNullOrWhiteSpace(secondCSVFile[i][j]) && !string.IsNullOrEmpty(secondCSVFile[i][j]) && string.Compare("", secondCSVFile[i][j], StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							if (firstCSVFile[i][j].Contains('"'))
								double.TryParse(firstCSVFile[i][j].Split('"').Skip(1).Take(1).FirstOrDefault(), out sub1);
							else
								double.TryParse(firstCSVFile[i][j], out sub1);

							if (secondCSVFile[i][j].Contains('"'))
								double.TryParse(secondCSVFile[i][j].Split('"').Skip(1).Take(1).FirstOrDefault(), out sub2);
							else
								double.TryParse(secondCSVFile[i][j], out sub2);

							diff = Math.Abs(sub1) - Math.Abs(sub2);

							if (diff != default(double))
							{
								IsCrucial = Math.Abs(diff) > m_minimalSmallNumber ? "CRUCIAL" : "Not Crucial";
							}
						}
                    }
                    finally
                    {
						if ((!string.IsNullOrWhiteSpace(firstCSVFile[i][j]) && !string.IsNullOrEmpty(firstCSVFile[i][j]) && string.Compare("", firstCSVFile[i][j], StringComparison.InvariantCultureIgnoreCase) != 0)
							&& !string.IsNullOrWhiteSpace(secondCSVFile[i][j]) && !string.IsNullOrEmpty(secondCSVFile[i][j]) && string.Compare("", secondCSVFile[i][j], StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							if (m_settings.IsWriteOnlyCrucail)
							{
								if (IsCrucial == "CRUCIAL")
									BuildString(firstCSVFile[0][j], firstCSVFile[i][j], secondCSVFile[i][j], diff, IsCrucial);
							}
							else
							{
								if (IsCrucial == "CRUCIAL")
								{
									BuildString(firstCSVFile[0][j], firstCSVFile[i][j], secondCSVFile[i][j], diff, IsCrucial);
								}
								else
								{
									BuildString(firstCSVFile[0][j], firstCSVFile[i][j], secondCSVFile[i][j], diff, IsCrucial);
								}
							}
						}
                    }
                }

                if (j == firstCSVFile[0].Length - 1)
                {
                    i++;
                    j = 0;
                }
            }

            m_stringBuilder.AppendLine(msg);
            return m_stringBuilder;
        }

        private void BuildString(string columnName, string firstCSVFileCell, string secondFileName, double diff, string isCrucial)
        {
            string sbMsg = $@"Column Name is: {columnName} || file name is: {m_settings.FirstFileName} value is: {firstCSVFileCell} || file name is: {m_settings.SecondFileName} value is: {secondFileName} diff is: {diff}. || The DIFF is {isCrucial ?? isCrucial: ''}";

            if (string.Compare(isCrucial, "CRUCIAL") == 0)
            {
                Log.Error(sbMsg);
            }
            else
            {
                Log.Information(sbMsg);
            }

            m_stringBuilder.AppendLine(sbMsg);
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
