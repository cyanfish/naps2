using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    public static class PathHelper
    {
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
