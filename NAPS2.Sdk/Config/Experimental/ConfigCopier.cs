using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAPS2.Util;

namespace NAPS2.Config.Experimental
{
    public static class ConfigCopier
    {
        private static readonly Dictionary<Type, (PropertyInfo, bool)[]> PropCache = new Dictionary<Type, (PropertyInfo, bool)[]>();

        public static void Copy<T>(T src, T dst) =>
            Copy(src, dst, typeof(T));

        private static void Copy(object src, object dst, Type t)
        {
            (PropertyInfo, bool)[] GetPropData() => t.GetProperties()
                .Select(x => (x, IsChildProp(x))).ToArray();

            foreach (var (prop, isChild) in PropCache.Get(t, GetPropData))
            {
                if (isChild)
                {
                    Copy(prop.GetValue(src), prop.GetValue(dst), prop.PropertyType);
                }
                else
                {
                    prop.SetValue(dst, prop.GetValue(src));
                }
            }
        }

        private static bool IsChildProp(PropertyInfo x)
        {
            return x.GetCustomAttributes().Any(y => y is ChildAttribute);
        }
    }
}
