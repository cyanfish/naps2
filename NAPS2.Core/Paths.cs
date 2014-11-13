/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

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
        private static readonly string AppDataPath = ExecutablePath;
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
