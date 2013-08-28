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

namespace NAPS2.ImportExport.Images
{
    /// <summary>
    /// Determines file names for images from a set of scans based on the number of images (e.g. abc1.jpg vs abc001.jpg).
    /// </summary>
    public class ImageFileNamer
    {
        public IEnumerable<string> GetFileNames(string baseFileName, int imageCount)
        {
            if (imageCount == 0)
            {
                // Skip the unnecesary logic below if there are no images
                return Enumerable.Empty<string>();
            }

            // Split the baseFileName into two parts, between which the number (if any) will be placed
            string name = Path.GetFileNameWithoutExtension(baseFileName);
            string dir = Path.GetDirectoryName(baseFileName);
            string prefix = string.IsNullOrEmpty(dir) ? name : dir + "\\" + name;
            string postfix = Path.GetExtension(baseFileName);

            if (imageCount == 1)
            {
                // Don't show any number at all if there's only one image
                return Enumerable.Repeat(prefix + postfix, 1);
            }

            // The number of digits in each number (everything should be zero-padded to this)
            // Based on the number of images, e.g. (2-9 images -> 1 digit, 10-99 -> 2, 100-999 -> 3, etc.)
            int digits = (int)Math.Floor(Math.Log10(imageCount)) + 1;

            return Enumerable.Range(1, imageCount).Select(i => prefix + i.ToString("D" + digits) + postfix);
        }
    }
}
