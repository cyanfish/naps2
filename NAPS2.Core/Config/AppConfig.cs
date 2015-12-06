/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Update;

namespace NAPS2.Config
{
    public class AppConfig
    {
        public const int CURRENT_VERSION = 2;

        public int Version { get; set; }

        public string DefaultCulture { get; set; }

        public string StartupMessageTitle { get; set; }

        public string StartupMessageText { get; set; }

        public MessageBoxIcon StartupMessageIcon { get; set; }

        public ScanProfile DefaultProfileSettings { get; set; }

        public AutoUpdateStatus AutoUpdate { get; set; }
    }
}
