using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Recovery;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

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
            lock (this)
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
        }

        public IEnumerable<int> MoveDown(IEnumerable<int> selection)
        {
            lock (this)
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
        }

        public IEnumerable<int> MoveTo(IEnumerable<int> selection, int index)
        {
            lock (this)
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
        }

        public void Delete(IEnumerable<int> selection)
        {
            lock (this)
            {
                using (RecoveryImage.DeferSave())
                {
                    foreach (ScannedImage img in Images.ElementsAt(selection))
                    {
                        img.Dispose();
                    }
                    Images.RemoveAll(selection);
                }
            }
        }

        public IEnumerable<int> Interleave(IEnumerable<int> selection)
        {
            lock (this)
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

                RecoveryImage.Refresh(Images);

                // Clear the selection (may be changed in the future to maintain it, but not necessary)
                return Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> Deinterleave(IEnumerable<int> selection)
        {
            lock (this)
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

                RecoveryImage.Refresh(Images);

                // Clear the selection (may be changed in the future to maintain it, but not necessary)
                return Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> AltInterleave(IEnumerable<int> selectedIndices)
        {
            lock (this)
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

                RecoveryImage.Refresh(Images);

                // Clear the selection (may be changed in the future to maintain it, but not necessary)
                return Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> AltDeinterleave(IEnumerable<int> selectedIndices)
        {
            lock (this)
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

                RecoveryImage.Refresh(Images);

                // Clear the selection (may be changed in the future to maintain it, but not necessary)
                return Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> Reverse()
        {
            lock (this)
            {
                Reverse(Enumerable.Range(0, Images.Count));

                // Selection is unpredictable, so clear it
                return Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> Reverse(IEnumerable<int> selection)
        {
            lock (this)
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
        }

        public async Task RotateFlip(IEnumerable<int> selection, RotateFlipType rotateFlipType)
        {
            var images = Images.ElementsAt(selection).ToList();
            await Task.Factory.StartNew(() =>
            {
                foreach (ScannedImage img in images)
                {
                    lock (img)
                    {
                        var transform = new RotationTransform(rotateFlipType);
                        img.AddTransform(transform);
                        var thumb = img.GetThumbnail();
                        if (thumb != null)
                        {
                            img.SetThumbnail(transform.Perform(thumb));
                        }
                    }
                }
            });
        }

        public void ResetTransforms(IEnumerable<int> selection)
        {
            foreach (ScannedImage img in Images.ElementsAt(selection))
            {
                img.ResetTransforms();
            }
        }
    }
}
