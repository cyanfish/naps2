/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace NAPS
{
    public class CPageSizes
    {
        public enum PageSize
        {
            A5,
            A4,
            A3,
            LEGAL,
            LETTER
        }

        public static string[] GetPageSizeList()
        {
            return new string[] { "A5 (148x210 mm)", "A4 (210x297 mm)", "A3 (297x420 mm)", "US LEGAL (8.5x14 in)", "US LETTER (8.5x11 in)" };
        }

        public static Size TranslatePageSize(PageSize pageSize)
        {
            List<Size> sizes = new List<Size>();
            sizes.Add(new Size(5826, 8267));
            sizes.Add(new Size(8267, 11692));
            sizes.Add(new Size(11692, 16535));
            sizes.Add(new Size(8500, 14000));
            sizes.Add(new Size(8500, 11000));

            return sizes[(int)pageSize];
        }
    }
}
