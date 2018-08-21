using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Sane
{
    public class SaneOptionCollection : Dictionary<string, SaneOption>
    {
        public void Add(SaneOption option)
        {
            Add(option.Name, option);
        }
    }
}
