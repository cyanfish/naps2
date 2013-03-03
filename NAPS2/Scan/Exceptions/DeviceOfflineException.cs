/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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

namespace NAPS2.Scan
{
    public class DeviceOfflineException : ScanDriverException
    {
        private const string DEFAULT_MESSAGE = "The selected scanner is offline.";

        public DeviceOfflineException()
            : base(DEFAULT_MESSAGE)
        {
        }

        public DeviceOfflineException(string message)
            : base(message)
        {
        }

        public DeviceOfflineException(Exception innerException)
            : base(DEFAULT_MESSAGE, innerException)
        {
        }

        public DeviceOfflineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
