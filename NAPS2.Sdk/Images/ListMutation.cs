using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images
{
    public abstract class ListMutation<T>
    {
        public abstract void Apply(List<T> list, ref ListSelection<T> selection);

        public class MoveDown : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                int upperBound = list.Count - 1;
                foreach (int i in selection.ToSelectedIndices(list).Reverse())
                {
                    if (i != upperBound--)
                    {
                        var item = list[i];
                        list.RemoveAt(i);
                        list.Insert(i + 1, item);
                    }
                }
            }
        }

        public class MoveUp : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                int lowerBound = 0;
                foreach (int i in selection.ToSelectedIndices(list))
                {
                    if (i != lowerBound++)
                    {
                        var item = list[i];
                        list.RemoveAt(i);
                        list.Insert(i - 1, item);
                    }
                }
            }
        }

        public class MoveTo : ListMutation<T>
        {
            private readonly int destinationIndex;

            public MoveTo(int destinationIndex)
            {
                this.destinationIndex = destinationIndex;
            }
            
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                var indexList = selection.ToSelectedIndices(list).ToList();
                var bottom = indexList.Where(x => x < destinationIndex).OrderByDescending(x => x).ToList();
                var top = indexList.Where(x => x >= destinationIndex).OrderBy(x => x).ToList();

                int offset = 1;
                foreach (int i in bottom)
                {
                    var item = list[i];
                    list.RemoveAt(i);
                    list.Insert(destinationIndex - offset, item);
                    offset++;
                }

                offset = 0;
                foreach (int i in top)
                {
                    var item = list[i];
                    list.RemoveAt(i);
                    list.Insert(destinationIndex + offset, item);
                    offset++;
                }
            }
        }

        public class Interleave : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                // Partition the image list in two
                int count = list.Count;
                int split = (count + 1) / 2;
                var p1 = list.Take(split).ToList();
                var p2 = list.Skip(split).ToList();

                // Rebuild the image list, taking alternating images from each the partitions
                list.Clear();
                for (int i = 0; i < count; ++i)
                {
                    list.Add(i % 2 == 0 ? p1[i / 2] : p2[i / 2]);
                }

                selection = ListSelection.Empty<T>();
            }
        }

        public class Deinterleave : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                // Duplicate the list
                int count = list.Count;
                int split = (count + 1) / 2;
                var copy = list.ToList();

                // Rebuild the image list, even-indexed images first
                list.Clear();
                for (int i = 0; i < split; ++i)
                {
                    list.Add(copy[i * 2]);
                }

                for (int i = 0; i < (count - split); ++i)
                {
                    list.Add(copy[i * 2 + 1]);
                }

                selection = ListSelection.Empty<T>();
            }
        }

        public class AltInterleave : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                // Partition the image list in two
                int count = list.Count;
                int split = (count + 1) / 2;
                var p1 = list.Take(split).ToList();
                var p2 = list.Skip(split).ToList();

                // Rebuild the image list, taking alternating images from each the partitions (the latter in reverse order)
                list.Clear();
                for (int i = 0; i < count; ++i)
                {
                    list.Add(i % 2 == 0 ? p1[i / 2] : p2[p2.Count - 1 - i / 2]);
                }

                selection = ListSelection.Empty<T>();
            }
        }

        public class AltDeinterleave : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                // Duplicate the list
                int count = list.Count;
                int split = (count + 1) / 2;
                var copy = list.ToList();

                // Rebuild the image list, even-indexed images first (odd-indexed images in reverse order)
                list.Clear();
                for (int i = 0; i < split; ++i)
                {
                    list.Add(copy[i * 2]);
                }

                for (int i = count - split - 1; i >= 0; --i)
                {
                    list.Add(copy[i * 2 + 1]);
                }

                selection = ListSelection.Empty<T>();
            }
        }

        public class ReverseAll : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                list.Reverse();
            }
        }

        public class ReverseSelection : ListMutation<T>
        {
            public override void Apply(List<T> list, ref ListSelection<T> selection)
            {
                var indexList = selection.ToSelectedIndices(list).ToList();
                int pairCount = indexList.Count / 2;

                // Swap pairs in the selection, excluding the middle element (if the total count is odd)
                for (int i = 0; i < pairCount; i++)
                {
                    int x = indexList[i];
                    int y = indexList[indexList.Count - i - 1];
                    (list[x], list[y]) = (list[y], list[x]);
                }
            }
        }
    }
}
