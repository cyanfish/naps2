/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using System.Text;
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

        private static readonly string TempPath = Path.Combine(AppDataPath, "tmp");

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

        public static string Executable
        {
            get
            {
                if (!Directory.Exists(ExecutablePath))
                {
                    Directory.CreateDirectory(ExecutablePath);
                }
                return ExecutablePath;
            }
        }

        public static string Temp
        {
            get
            {
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }
                return TempPath;
            }
        }
    }
}
