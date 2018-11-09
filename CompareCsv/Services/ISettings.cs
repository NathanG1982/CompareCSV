using System;
using System.Collections.Generic;
using System.Text;

namespace CompareCsv.Services
{
    public interface ISettings
    {
        string FirstFileName { get;}

        string SecondFileName { get;}

        bool IsDeleteFiles { get;}

        bool IgnoreWindData { get; set; }
    }
}
