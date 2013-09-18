using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPS2.Update
{
    public interface ILatestVersionSource
    {
        Task<string> GetLatestVersion();
    }
}
