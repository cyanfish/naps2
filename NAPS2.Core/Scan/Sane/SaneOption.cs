using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Sane
{
    public class SaneOption
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Desc { get; set; }

        public SaneValueType Type { get; set; }

        public SaneUnit Unit { get; set; }

        public int Size { get; set; }

        public SaneCapabilities Capabilities { get; set; }

        public SaneConstraintType ConstraintType { get; set; }

        public List<string> StringList { get; set; }

        public List<decimal> WordList { get; set; }

        public SaneRange Range { get; set; }

        public decimal CurrentNumericValue { get; set; }

        public string CurrentStringValue { get; set; }
    }
}
