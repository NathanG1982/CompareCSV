using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CompareCsv.Services
{
    public interface ISettings
    {
		string BaseDir { get; }

		string FirstFileName { get;}

        string SecondFileName { get;}

        bool IsDeleteFiles { get;}

        bool IgnoreWindData { get; set; }
    }
}
