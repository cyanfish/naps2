using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2
{
    public static class CollectionExtensions
    {
        public static void RemoveAll<T>(this IList<T> list, IEnumerable<int> indices)
        {
            int offset = 0;
            foreach (int i in indices.OrderBy(x => x))
            {
                list.RemoveAt(i - offset++);
            }
        }

        public static IEnumerable<T> ElementsAt<T>(this IList<T> list, IEnumerable<int> indices)
        {
            return indices.Select(i => list[i]);
        }
    }
}
