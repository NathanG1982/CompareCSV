using CompareCsv.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompareCsv
{
    public class Settings: ISettings
    {
        public string FirstFileName { get; set; }

        public string SecondFileName { get; set; }

        public bool IsDeleteFiles { get; set; } = false;

        public bool IgnoreWindData { get; set; } = true;
    }
}
