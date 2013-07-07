using System;
using System.Collections.Generic;
#if !STANDALONE
using System.IO;
#endif
using System.Linq;
using System.Text;
#if STANDALONE
using System.Windows.Forms;
#endif

namespace NAPS2
{
    public static class Paths
    {
#if STANDALONE
        private static readonly string AppDataPath = Application.StartupPath;
#else
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NAPS2");
#endif
        public static string AppData
        {
            get
            {
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }
                return AppDataPath;
            }
        }
    }
}
