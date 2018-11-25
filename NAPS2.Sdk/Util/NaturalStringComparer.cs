using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }
            int i, j;
            for (i = 0, j = 0; i < x.Length && j < y.Length; i++, j++)
            {
                if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
                {
                    // Number comparison
                    double xn = 0, yn = 0;

                    for (; i < x.Length && char.IsDigit(x[i]); i++)
                    {
                        double val = char.GetNumericValue(x[i]);
                        if (val > 0 || xn > 0)
                        {
                            xn = xn * 10 + val;
                        }
                    }
                    i -= 1;

                    for (; j < y.Length && char.IsDigit(y[j]); j++)
                    {
                        double val = char.GetNumericValue(y[j]);
                        if (val > 0 || yn > 0)
                        {
                            yn = yn * 10 + val;
                        }
                    }
                    j -= 1;

                    if (xn < yn)
                    {
                        return -1;
                    }
                    if (xn > yn)
                    {
                        return 1;
                    }
                }
                int result = string.Compare(x[i].ToString(), y[j].ToString(), StringComparison.CurrentCultureIgnoreCase);
                if (result != 0)
                {
                    return result;
                }
            }
            return x.Length - y.Length;
        }
    }
}
