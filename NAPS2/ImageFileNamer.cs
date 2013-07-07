using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2
{
    /// <summary>
    /// Determines file names for images from a set of scans based on the number of images (e.g. abc1.jpg vs abc001.jpg).
    /// </summary>
    public class ImageFileNamer
    {
        public IEnumerable<string> GetFileNames(string baseFileName, int imageCount)
        {
            // Get the strings that surround the number (if any)
            string prefix = Path.GetDirectoryName(baseFileName) + "\\" + Path.GetFileNameWithoutExtension(baseFileName);
            string postfix = Path.GetExtension(baseFileName);

            int digits = 0; // The number of digits in each number (everything should be zero-padded to this)
            if (imageCount > 1) // Don't show any digits at all if there's only one image
            {
                // Otherwise, use the number of digits in the number (2-9 -> 1, 10-99 -> 2, 100-999 -> 3, etc.)
                digits = (int) Math.Floor(Math.Log10(digits)) + 1;
            }

            return Enumerable.Range(1, imageCount).Select(i => i.ToString("D" + digits));
        }
    }
}
