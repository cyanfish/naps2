using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2
{
    public static class Paths
    {
        private static readonly string ExecutablePath = Application.StartupPath;

#if STANDALONE
        private static readonly string AppDataPath = Path.Combine(ExecutablePath, @"..\Data");
#else
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NAPS2");
#endif

        private static readonly string TempPath = Path.Combine(AppDataPath, "temp");

        private static readonly string RecoveryPath = Path.Combine(AppDataPath, "recovery");

        private static readonly string ComponentsPath = Path.Combine(AppDataPath, "components");

        public static string AppData
        {
            get
            {
                return EnsureFolderExists(AppDataPath);
            }
        }

        public static string Executable
        {
            get
            {
                return EnsureFolderExists(ExecutablePath);
            }
        }

        public static string Temp
        {
            get
            {
                return EnsureFolderExists(TempPath);
            }
        }

        public static string Recovery
        {
            get
            {
                return EnsureFolderExists(RecoveryPath);
            }
        }

        public static string Components
        {
            get
            {
                return EnsureFolderExists(ComponentsPath);
            }
        }

        private static string EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
    }
}
