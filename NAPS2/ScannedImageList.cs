using NAPS2.Scan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2
{
    public class ScannedImageList
    {

        public ScannedImageList()
        {
            Images = new List<IScannedImage>();
        }

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
                    var img = Images[i];
                    Images.RemoveAt(i);
                    Images.Insert(i - 1, img);
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
                    var img = Images[i];
                    Images.RemoveAt(i);
                    Images.Insert(i + 1, img);
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
                Images[i].RotateFlip(rotateFlipType);
            }
            return selection.ToList();
        }

        public void Delete(IEnumerable<int> selection)
        {
            foreach (int i in selection.OrderBy(x => x))
            {
                Images[i].Dispose();
                Images.RemoveAt(i);
            }
        }

    }
}
