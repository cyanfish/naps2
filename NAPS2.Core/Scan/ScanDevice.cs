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

namespace NAPS2.Scan
{
    public class ScanDevice
    {
        public ScanDevice(string id, string name)
        {
            ID = id;
            Name = name;
        }

        public ScanDevice()
        {
        }

        public string ID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// This property only exists for compatibility when reading profiles.xml from an older version. Use ScanProfile.DriverName instead.
        /// </summary>
        public string DriverName { get; set; }
    }
}
