using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Update
{
    public interface IAutoInstaller
    {
        void DownloadAndInstall(string version); // TODO: Maybe file name instead
    }
}
