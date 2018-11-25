using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Localization
{
    public class TranslatableString
    {
        public string Original { get; set; }

        public string Translation { get; set; }

        public List<string> Context { get; set; }
    }
}