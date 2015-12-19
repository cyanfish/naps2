/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Luka Kama
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
using System.Drawing;
using System.Linq;
using NAPS2.Config;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Scan.Images
{
    public class ScannedImageList
    {
        public ScannedImageList()
        {
            Images = new List<IScannedImage>();
        }

        public IUserConfigManager UserConfigManager { get; set; }

        public List<IScannedImage> Images { get; private set; }

        public IEnumerable<int> MoveUp(IEnumerable<int> selection)
        {
            var newSelection = new int[selection.Count()];
            int lowerBound = 0;
            int j = 0;
            foreach (int i in selection.OrderBy(x => x))
            {
                if (i != lowerBound++)
                {
                    IScannedImage img = Images[i];
                    Images.RemoveAt(i);
                    Images.Insert(i - 1, img);
                    img.MovedTo(i - 1);
                    newSelection[j++] = i - 1;
                }
                else
                {
                    newSelection[j++] = i;
                }
            }
            return newSelection;
        }

        public IEnumerable<int> MoveDown(IEnumerable<int> selection)
        {
            var newSelection = new int[selection.Count()];
            int upperBound = Images.Count - 1;
            int j = 0;
            foreach (int i in selection.OrderByDescending(x => x))
            {
                if (i != upperBound--)
                {
                    IScannedImage img = Images[i];
                    Images.RemoveAt(i);
                    Images.Insert(i + 1, img);
                    img.MovedTo(i + 1);
                    newSelection[j++] = i + 1;
                }
                else
                {
                    newSelection[j++] = i;
                }
            }
            return newSelection;
        }

        public IEnumerable<int> RotateFlip(IEnumerable<int> selection, RotateFlipType rotateFlipType)
        {
            foreach (int i in selection)
            {
                Images[i].AddTransform(new RotationTransform(rotateFlipType));
                Images[i].SetThumbnail(Images[i].RenderThumbnail(UserConfigManager.Config.ThumbnailSize));
            }
            return selection.ToList();
        }

        public void Delete(IEnumerable<int> selection)
        {
            foreach (IScannedImage img in Images.ElementsAt(selection))
            {
                img.Dispose();
            }
            Images.RemoveAll(selection);
        }

        public IEnumerable<int> Interleave(IEnumerable<int> selection)
        {
            // Partition the image list in two
            int count = Images.Count;
            int split = (count + 1) / 2;
            var p1 = Images.Take(split).ToList();
            var p2 = Images.Skip(split).ToList();

            // Rebuild the image list, taking alternating images from each the partitions
            Images.Clear();
            for (int i = 0; i < count; ++i)
            {
                Images.Add(i % 2 == 0 ? p1[i / 2] : p2[i / 2]);
            }

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> Deinterleave(IEnumerable<int> selection)
        {
            // Duplicate the list
            int count = Images.Count;
            int split = (count + 1) / 2;
            var images = Images.ToList();

            // Rebuild the image list, even-indexed images first
            Images.Clear();
            for (int i = 0; i < split; ++i)
            {
                Images.Add(images[i * 2]);
            }
            for (int i = 0; i < (count - split); ++i)
            {
                Images.Add(images[i * 2 + 1]);
            }

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> AltInterleave(IEnumerable<int> selectedIndices)
        {
            // Partition the image list in two
            int count = Images.Count;
            int split = (count + 1) / 2;
            var p1 = Images.Take(split).ToList();
            var p2 = Images.Skip(split).ToList();

            // Rebuild the image list, taking alternating images from each the partitions (the latter in reverse order)
            Images.Clear();
            for (int i = 0; i < count; ++i)
            {
                Images.Add(i % 2 == 0 ? p1[i / 2] : p2[p2.Count - 1 - i / 2]);
            }

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> AltDeinterleave(IEnumerable<int> selectedIndices)
        {
            // Duplicate the list
            int count = Images.Count;
            int split = (count + 1) / 2;
            var images = Images.ToList();

            // Rebuild the image list, even-indexed images first (odd-indexed images in reverse order)
            Images.Clear();
            for (int i = 0; i < split; ++i)
            {
                Images.Add(images[i * 2]);
            }
            for (int i = count - split - 1; i >= 0; --i)
            {
                Images.Add(images[i * 2 + 1]);
            }

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> Reverse()
        {
            Reverse(Enumerable.Range(0, Images.Count));

            // Selection is unpredictable, so clear it
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> Reverse(IEnumerable<int> selection)
        {
            var selectionList = selection.ToList();
            int pairCount = selectionList.Count / 2;

            // Swap pairs in the selection, excluding the middle element (if the total count is odd)
            for (int i = 0; i < pairCount; i++)
            {
                int x = selectionList[i];
                int y = selectionList[selectionList.Count - i - 1];
                var temp = Images[x];
                Images[x] = Images[y];
                Images[y] = temp;
            }

            // Selection stays the same, so is easy to maintain
            return selectionList;
        }

        public IEnumerable<int> ResetTransforms(IEnumerable<int> selection)
        {
            foreach (IScannedImage img in Images.ElementsAt(selection))
            {
                img.ResetTransforms();
                img.SetThumbnail(img.RenderThumbnail(UserConfigManager.Config.ThumbnailSize));
            }
            return selection.ToList();
        }
    }
}
