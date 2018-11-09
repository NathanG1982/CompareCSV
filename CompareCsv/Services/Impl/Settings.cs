using CompareCsv.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CompareCsv
{
    public class Settings: ISettings
    {
		public string BaseDir { get; set; } = Directory.GetCurrentDirectory();

		public string FirstFileName { get; set; }

        public string SecondFileName { get; set; }

        public bool IsDeleteFiles { get; set; } = false;

        public bool IgnoreWindData { get; set; } = true;

        public bool IsWriteOnlyCrucail { get; set; } = false;
    }
}
