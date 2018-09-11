using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A common signature for progress handling.
    /// </summary>
    /// <param name="current">The number of completed items.</param>
    /// <param name="max">The total number of items.</param>
    public delegate void ProgressHandler(int current, int max);
}
