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
using NAPS2.Lang.Resources;

namespace NAPS2.Scan.Exceptions
{
    public class DeviceOfflineException : ScanDriverException
    {
        public DeviceOfflineException()
            : base(MiscResources.DeviceOffline)
        {
        }

        public DeviceOfflineException(string message)
            : base(message)
        {
        }

        public DeviceOfflineException(Exception innerException)
            : base(MiscResources.DeviceOffline, innerException)
        {
        }

        public DeviceOfflineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
