using System;
using System.Linq;

namespace NAPS2.Wia
{
    public class WiaPropertyAttributes
    {
        public WiaPropertyAttributes(IntPtr storage, int id)
        {
            WiaException.Check(NativeWiaMethods.GetPropertyAttributes(storage, id, out int flags, out int min, out int nom, out int max, out int step, out _, out var elems));
            Flags = (WiaPropertyFlags) flags;
            Min = min;
            Nom = nom;
            Max = max;
            Step = step;
            Values = elems?.Skip(2).Cast<object>().ToArray();
        }

        public WiaPropertyFlags Flags { get; }

        public int Min { get; }

        public int Nom { get; }

        public int Max { get; }

        public int Step { get; }

        public object[]? Values { get; }
    }
}