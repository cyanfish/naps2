using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.WinForms;

namespace NAPS2.Scan
{
    public interface IScanDriverFactory
    {
        IScanDriver Create(string driverName);
    }
}
