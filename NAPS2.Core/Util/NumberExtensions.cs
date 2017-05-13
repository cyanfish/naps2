using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    public static class NumberExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }
            if (val.CompareTo(max) > 0)
            {
                return max;
            }
            return val;
        }
    }
}
