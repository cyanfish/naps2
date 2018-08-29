using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan
{
    /// <summary>
    /// A representation for which patch code, if any, has been detected on a document.
    /// http://www.alliancegroup.co.uk/patch-codes.htm
    /// </summary>
    public enum PatchCode
    {
        None,
        Patch1,
        Patch2,
        Patch3,
        Patch4,
        Patch6,
        PatchT
    }
}
