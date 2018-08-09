using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    /// <summary>
    /// A singleton class used to track whether the user has made unsaved changes, and therefore should be prompted before exiting.
    /// </summary>
    public class ChangeTracker
    {
        public bool HasUnsavedChanges { get; set; }
    }
}
