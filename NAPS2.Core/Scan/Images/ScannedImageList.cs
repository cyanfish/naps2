using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NAPS2.Scan.Images
{
    public class ScannedImageList
    {
        public ScannedImageList()
        {
            Images = new List<ScannedImage>();
        }

        public ScannedImageList(List<ScannedImage> images)
        {
            Images = images;
        }

        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        public List<ScannedImage> Images { get; }

        public IEnumerable<int> MoveUp(IEnumerable<int> selection)
        {
            var newSelection = new int[selection.Count()];
            int lowerBound = 0;
            int j = 0;
            foreach (int i in selection.OrderBy(x => x))
            {
                if (i != lowerBound++)
                {
                    ScannedImage img = Images[i];
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
                    ScannedImage img = Images[i];
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

        public IEnumerable<int> MoveTo(IEnumerable<int> selection, int index)
        {
            var selList = selection.ToList();
            var bottom = selList.Where(x => x < index).OrderByDescending(x => x).ToList();
            var top = selList.Where(x => x >= index).OrderBy(x => x).ToList();

            int offset = 1;
            foreach (int i in bottom)
            {
                ScannedImage img = Images[i];
                Images.RemoveAt(i);
                Images.Insert(index - offset, img);
                img.MovedTo(index - offset);
                offset++;
            }

            offset = 0;
            foreach (int i in top)
            {
                ScannedImage img = Images[i];
                Images.RemoveAt(i);
                Images.Insert(index + offset, img);
                img.MovedTo(index + offset);
                offset++;
            }

            return Enumerable.Range(index - bottom.Count, selList.Count);
        }

        public IEnumerable<int> RotateFlip(IEnumerable<int> selection, RotateFlipType rotateFlipType)
        {
            foreach (int i in selection)
            {
                Images[i].AddTransform(new RotationTransform(rotateFlipType));
                Images[i].SetThumbnail(ThumbnailRenderer.RenderThumbnail(Images[i]));
            }
            return selection.ToList();
        }

        public void Delete(IEnumerable<int> selection)
        {
            foreach (ScannedImage img in Images.ElementsAt(selection))
            {
                img.Dispose();
            }
            Images.RemoveAll(selection);
        }

        public IEnumerable<int> Interleave(IEnumerable<int> selection)
        {
            if (selection == null)
                throw new System.ArgumentNullException(nameof(selection));
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

            RecoveryImage.Refresh(Images);

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> Deinterleave(IEnumerable<int> selection)
        {
            if (selection == null)
                throw new System.ArgumentNullException(nameof(selection));
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
                Images.Add(images[(i * 2) + 1]);
            }

            RecoveryImage.Refresh(Images);

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> AltInterleave(IEnumerable<int> selectedIndices)
        {
            if (selectedIndices == null)
                throw new System.ArgumentNullException(nameof(selectedIndices));
            // Partition the image list in two
            int count = Images.Count;
            int split = (count + 1) / 2;
            var p1 = Images.Take(split).ToList();
            var p2 = Images.Skip(split).ToList();

            // Rebuild the image list, taking alternating images from each the partitions (the latter in reverse order)
            // TODO: verify this; the order of Operations wasn't set, and the results seemed off before making changes.
            Images.Clear();
            for (int i = 0; i < count; ++i)
            {
                Images.Add(i % 2 == 0 ? p1[i / 2] : p2[p2.Count - 1 - (i / 2)]);
            }

            RecoveryImage.Refresh(Images);

            // Clear the selection (may be changed in the future to maintain it, but not necessary)
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> AltDeinterleave(IEnumerable<int> selectedIndices)
        {
            if (selectedIndices == null)
                throw new System.ArgumentNullException(nameof(selectedIndices));
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
                Images.Add(images[(i * 2) + 1]);
            }

            RecoveryImage.Refresh(Images);

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

            RecoveryImage.Refresh(Images);

            // Selection stays the same, so is easy to maintain
            return selectionList;
        }

        public IEnumerable<int> ResetTransforms(IEnumerable<int> selection)
        {
            foreach (ScannedImage img in Images.ElementsAt(selection))
            {
                img.ResetTransforms();
                img.SetThumbnail(ThumbnailRenderer.RenderThumbnail(img));
            }
            return selection.ToList();
        }
    }
}