using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NAPS2.Util
{
    /// <summary>
    /// A class that represents a Python-style slice of a collection.
    /// https://stackoverflow.com/questions/509211/understanding-pythons-slice-notation/509295#509295
    ///
    /// For example, "2:-3" gets all but the first 2 and last 3 items.
    /// Stepping is supported too, as in "::2" which gets every other item.
    /// </summary>
    public class Slice
    {
        public static readonly Slice Default = new Slice(null, null, null);

        private static readonly Regex ParseRegex = new Regex(@"^(.*)\[([^\[\]]*)\](\s*)$");

        public static Slice Parse(string input, out string rest)
        {
            var match = ParseRegex.Match(input);
            if (!match.Success)
            {
                rest = input;
                return Default;
            }
            var sliceStr = match.Groups[2].Value;
            rest = match.Groups[1].Value + match.Groups[3].Value;
            var parts = sliceStr.Split(':');

            int? start = null, end = null, step = null;

            if (parts.Length == 1)
            {
                if (int.TryParse(parts[0], out int s))
                {
                    return Item(s);
                }
                return Item(null);
            }
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out int s))
                {
                    start = s;
                }
                if (int.TryParse(parts[1], out int e))
                {
                    end = e;
                }
            }
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[2], out int s))
                {
                    step = s;
                }
            }

            return Range(start, end, step);
        }

        public static Slice Item(int? index)
        {
            return new Slice(index);
        }

        public static Slice Range(int? start, int? end, int? step)
        {
            return new Slice(start, end, step);
        }

        private Slice(int? index)
        {
            Index = index;
        }

        private Slice(int? start, int? end, int? step)
        {
            Start = start;
            End = end;
            Step = step;
        }

        public int? Index { get; }

        public int? Start { get; }

        public int? End { get; }

        public int? Step { get; }

        public IEnumerable<int> Indices(int collectionLength)
        {
            if (Index.HasValue)
            {
                int i = Index.Value < 0 ? collectionLength + Index.Value : Index.Value;
                if (i >= 0 && i < collectionLength)
                {
                    yield return i;
                }
                yield break;
            }

            bool reverse = Step.HasValue && Step.Value < 0;
            int start, end, step;
            if (Start.HasValue)
            {
                start = Start.Value < 0 ? collectionLength + Start.Value : Start.Value;
            }
            else
            {
                start = reverse ? collectionLength - 1 : 0;
            }
            if (End.HasValue)
            {
                end = End.Value < 0 ? collectionLength + End.Value : End.Value;
            }
            else
            {
                end = reverse ? -1 : collectionLength;
            }
            if (Step.HasValue)
            {
                step = Step.Value == 0 ? 1 : Step.Value;
            }
            else
            {
                step = 1;
            }

            if (reverse)
            {
                for (int i = Math.Min(start, collectionLength - 1); i > Math.Max(end, -1); i += step)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = Math.Max(start, 0); i < Math.Min(end, collectionLength); i += step)
                {
                    yield return i;
                }
            }
        }
    }
}
