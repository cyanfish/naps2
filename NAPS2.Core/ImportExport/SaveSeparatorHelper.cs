using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    public static class SaveSeparatorHelper
    {
        /// <summary>
        /// Given a list of scans (each of which is a list of 1 or more images),
        /// split up the images into multiple lists as described by the SaveSeparator parameter.
        /// </summary>
        /// <param name="scans"></param>
        /// <param name="separator"></param>
        /// <param name="splitSize"></param>
        /// <returns></returns>
        public static IEnumerable<List<ScannedImage>> SeparateScans(IEnumerable<IEnumerable<ScannedImage>> scans, SaveSeparator separator, int splitSize = 1)
        {
            if (separator == SaveSeparator.FilePerScan)
            {
                foreach (var scan in scans)
                {
                    yield return scan.ToList();
                }
            }
            else if (separator == SaveSeparator.FilePerPage)
            {
                splitSize = Math.Max(splitSize, 1);
                foreach (var scan in scans.Select(x => x.ToList()))
                {
                    for (int i = 0; i < scan.Count; i += splitSize)
                    {
                        yield return scan.Skip(i).Take(splitSize).ToList();
                    }
                }
            }
            else if (separator == SaveSeparator.PatchT)
            {
                var images = new List<ScannedImage>();
                foreach (var scan in scans)
                {
                    foreach (var image in scan)
                    {
                        if (image.PatchCode == PatchCode.PatchT)
                        {
                            image.Dispose();
                            if (images.Count > 0)
                            {
                                yield return images;
                                images = new List<ScannedImage>();
                            }
                        }
                        else
                        {
                            images.Add(image);
                        }
                    }
                }
                if (images.Count > 0)
                {
                    yield return images;
                }
            }
            else
            {
                yield return scans.SelectMany(x => x.ToList()).ToList();
            }
        }
    }
}
