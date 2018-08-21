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

    public enum SaneValueType
    {
        None,
        Bool,
        Numeric,
        String,
        Button,
        Group
    }

    public enum SaneUnit
    {
        None,
        Pixel,
        Bit,
        Mm,
        Dpi,
        Percent,
        Microsecond
    }

    [Flags]
    public enum SaneCapabilities
    {
        None = 0,
        SoftSelect = 1,
        HardSelect = 2,
        SoftDetect = 4,
        Emulated = 8,
        Automatic = 16,
        Inactive = 32,
        Advanced = 64
    }

    public enum SaneConstraintType
    {
        None,
        Range,
        WordList,
        StringList
    }

    public class SaneRange
    {
        public decimal Min { get; set; }

        public decimal Max { get; set; }

        public decimal Quant { get; set; }
    }
}
