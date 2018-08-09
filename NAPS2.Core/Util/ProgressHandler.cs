using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    /// <summary>
    /// A common signature for progress handling.
    /// </summary>
    /// <param name="current">The number of completed items.</param>
    /// <param name="max">The total number of items.</param>
    /// <returns>True if the operation should continue, false if it should abort.</returns>
    public delegate bool ProgressHandler(int current, int max);
}
