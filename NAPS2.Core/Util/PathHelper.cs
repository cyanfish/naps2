using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Util
{
    public static class PathHelper
    {
        /// <summary>
        /// Creates the parent directory for the provided path if needed.
        /// </summary>
        /// <param name="filePath"></param>
        public static void EnsureParentDirExists(string filePath)
        {
            var parentDir = new FileInfo(filePath).Directory;
            if (parentDir != null && !parentDir.Exists)
            {
                parentDir.Create();
            }
        }
    }
}
