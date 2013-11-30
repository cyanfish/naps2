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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2
{
    public static class CollectionExtensions
    {
        public static void RemoveAll(this IList list)
        {
            foreach (int i in Enumerable.Range(0, list.Count))
            {
                list.RemoveAt(0);
            }
        }

        public static void RemoveAll(this IList list, IEnumerable<int> indices)
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
