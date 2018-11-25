using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NAPS2.ImportExport
{
    public class FileNamePlaceholders
    {
        public const string YEAR_4_DIGITS = "$(YYYY)";
        public const string YEAR_2_DIGITS = "$(YY)";
        public const string MONTH_2_DIGITS = "$(MM)";
        public const string DAY_2_DIGITS = "$(DD)";
        public const string HOUR_24_CLOCK = "$(hh)";
        public const string MINUTE_2_DIGITS = "$(mm)";
        public const string SECOND_2_DIGITS = "$(ss)";
        public const string NUMBER_4_DIGITS = "$(nnnn)";
        public const string NUMBER_3_DIGITS = "$(nnn)";
        public const string NUMBER_2_DIGITS = "$(nn)";
        public const string NUMBER_1_DIGIT = "$(n)";

        private static readonly Dictionary<string, Func<DateTime, string>> Placeholders = new Dictionary<string, Func<DateTime, string>>
        {
            { YEAR_4_DIGITS, dateTime => dateTime.ToString("yyyy") },
            { YEAR_2_DIGITS, dateTime => dateTime.ToString("yy") },
            { MONTH_2_DIGITS, dateTime => dateTime.ToString("MM") },
            { DAY_2_DIGITS, dateTime => dateTime.ToString("dd") },
            { HOUR_24_CLOCK, dateTime => dateTime.ToString("HH") },
            { MINUTE_2_DIGITS, dateTime => dateTime.ToString("mm") },
            { SECOND_2_DIGITS, dateTime => dateTime.ToString("ss") },
        };

        private static readonly Regex NumberPlaceholderPattern = new Regex(@"\$\(n+\)");

        public string SubstitutePlaceholders(string fileNameWithPath, DateTime dateTime, bool incrementIfExists = true, int numberSkip = 0, int autoNumberDigits = 0)
        {
            if (fileNameWithPath == null)
            {
                return null;
            }
            // Start with environment variables
            string result = Environment.ExpandEnvironmentVariables(fileNameWithPath);
            // Most placeholders don't need a special case
            result = Placeholders.Aggregate(result, (current, ph) => current.Replace(ph.Key, ph.Value(dateTime)));
            // One does, however
            var match = NumberPlaceholderPattern.Match(result);
            if (match.Success)
            {
                result = NumberPlaceholderPattern.Replace(result, "");
                result = SubstituteNumber(result, match.Index, match.Length - 3, numberSkip, true);
            }
            else if (autoNumberDigits > 0)
            {
                result = result.Insert(result.Length - Path.GetExtension(result).Length, ".");
                result = SubstituteNumber(result, result.Length - Path.GetExtension(result).Length, autoNumberDigits, numberSkip, incrementIfExists);
            }
            return result;
        }

        private string SubstituteNumber(string path, int insertionIndex, int minDigits, int skip, bool incrementIfExists)
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
