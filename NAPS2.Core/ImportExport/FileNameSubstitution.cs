using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NAPS2.ImportExport
{
    public class FileNameSubstitution
    {
        private static readonly Dictionary<string, Func<string>> Subs = new Dictionary<string, Func<string>>
        {
            { "$(YYYY)", () => DateTime.Now.ToString("yyyy") },
            { "$(YY)", () => DateTime.Now.ToString("yy") },
            { "$(MM)", () => DateTime.Now.ToString("MM") },
            { "$(DD)", () => DateTime.Now.ToString("dd") },
            { "$(hh)", () => DateTime.Now.ToString("HH") },
            { "$(mm)", () => DateTime.Now.ToString("mm") },
            { "$(ss)", () => DateTime.Now.ToString("ss") },
        };

        private static readonly Regex NumberSubPattern = new Regex(@"\$\(n+\)");

        public string SubstituteFileName(string fileNameWithPath, bool incrementIfExists = true, int numberSkip = 0, int autoNumberDigits = 0)
        {
            // Most subs don't need a special case
            string result = Subs.Aggregate(fileNameWithPath, (current, sub) => current.Replace(sub.Key, sub.Value()));
            // One does, however
            var match = NumberSubPattern.Match(result);
            if (match.Success)
            {
                result = NumberSubPattern.Replace(result, "");
                result = SubNumber(result, match.Index, match.Length - 3, numberSkip, incrementIfExists);
            }
            else if (autoNumberDigits > 0)
            {
                result = result.Insert(result.Length - Path.GetExtension(result).Length, ".");
                result = SubNumber(result, result.Length - Path.GetExtension(result).Length, autoNumberDigits, numberSkip, incrementIfExists);
            }
            return result;
        }

        private string SubNumber(string path, int insertionIndex, int minDigits, int skip, bool incrementIfExists)
        {
            string result;
            int i = skip;
            do
            {
                ++i;
                result = path.Insert(insertionIndex, i.ToString("D" + minDigits));
            } while (incrementIfExists && File.Exists(result));
            return result;
        }
    }
}
