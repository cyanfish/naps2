using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Removes multiple elements from the list at the specified indices.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="indices"></param>
        public static void RemoveAll(this IList list, IEnumerable<int> indices)
        {
            int offset = 0;
            foreach (int i in indices.OrderBy(x => x))
            {
                list.RemoveAt(i - offset++);
            }
        }

        /// <summary>
        /// Gets an enumerable of elements at the specified indices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public static IEnumerable<T> ElementsAt<T>(this IList<T> list, IEnumerable<int> indices)
        {
            return indices.Select(i => list[i]);
        }

        /// <summary>
        /// Gets an enumerable of indices of the specified elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static IEnumerable<int> IndiciesOf<T>(this IList<T> list, IEnumerable<T> elements)
        {
            return elements.Select(list.IndexOf);
        }

        /// <summary>
        /// Adds a key-value pair to the multi-dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddMulti<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new HashSet<TValue>();
            }
            dict[key].Add(value);
        }

        /// <summary>
        /// Adds an enumeration of key-value pairs to the multi-dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public static void AddMulti<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dict, TKey key, IEnumerable<TValue> values)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new HashSet<TValue>();
            }
            foreach (var value in values)
            {
                dict[key].Add(value);
            }
        }
    }
}
