using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NAPS2.ImportExport;

/// <summary>
/// Class for handling substitution of special values in file paths. For example, "$(YYYY)" can be substituted with the current year.
/// Use Placeholders.All for recommended substitutions. Alternatively, you can use Placeholders.Env or Placeholders.None if you prefer.
/// </summary>
public abstract class Placeholders
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

    /// <summary>
    /// Substitutes all the standard placeholders. For example, "$(YYYY)-$(MM)-$(DD) $(hh):$(mm):$(ss)" is substituted with the current date and time. Substitutes environment variables. Handles auto-numbering for multiple files,
    /// using the numeric placeholders ("$(n)", "$(nn)", "$(nnn)", or "$(nnnn)") if specified; otherwise, the number is appended to the file name.
    /// </summary>
    public static DefaultPlaceholders All => new DefaultPlaceholders();

    /// <summary>
    /// Substitutes environment variables in file names. Not recommended if you may be saving multiple files.
    /// </summary>
    public static EnvironmentPlaceholders Env => new EnvironmentPlaceholders();

    /// <summary>
    /// Does not make any changes to the file name. Not recommended if you may be saving multiple files.
    /// </summary>
    public static StubPlaceholders None => new StubPlaceholders();

    /// <summary>
    /// Performs substitutions on the given file path.
    /// </summary>
    /// <param name="filePath">The file path to perform substitutions on.</param>
    /// <param name="incrementIfExists">Whether to use an auto-incrementing file number to make the file name unique.</param>
    /// <param name="numberSkip">The file number will be at least one bigger than this value.</param>
    /// <param name="autoNumberDigits">The minimum number of digits in the file number. Only has an effect if the path does not contain a numeric placeholder like $(n) or $(nnn).</param>
    /// <returns>The file path with substitutions.</returns>
    [return: NotNullIfNotNull("filePath")]
    public abstract string? Substitute(string? filePath, bool incrementIfExists = true, int numberSkip = 0,
        int autoNumberDigits = 0);

    public class StubPlaceholders : Placeholders
    {
        public override string? Substitute(string? filePath, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0) => filePath;
    }

    public class EnvironmentPlaceholders : Placeholders
    {
        public override string? Substitute(string? filePath, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0)
        {
            if (filePath == null) return null;
            return Environment.ExpandEnvironmentVariables(filePath);
        }
    }

    public class DefaultPlaceholders : Placeholders
    {
        private static readonly Dictionary<string, Func<DateTime, string>> Replacements =
            new Dictionary<string, Func<DateTime, string>>
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

        private readonly DateTime? _dateTimeOverride;

        public DefaultPlaceholders(DateTime? dateTimeOverride = null)
        {
            _dateTimeOverride = dateTimeOverride;
        }

        /// <summary>
        /// Creates a copy of the DefaultPlaceholders object that will use the specified DateTime for date and time substitutions.
        /// </summary>
        /// <param name="dateTime">The date and time to use.</param>
        /// <returns>The new DefaultPlaceholders object.</returns>
        public DefaultPlaceholders WithDate(DateTime dateTime) => new DefaultPlaceholders(dateTime);

        [return: NotNullIfNotNull("filePath")]
        public override string? Substitute(string? filePath, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0)
        {
            if (filePath == null)
            {
                return null;
            }

            var dateTime = _dateTimeOverride ?? DateTime.Now;
            // Start with environment variables
            string result = Environment.ExpandEnvironmentVariables(filePath);
            // Most placeholders don't need a special case
            result = Replacements.Aggregate(result, (current, ph) => current.Replace(ph.Key, ph.Value(dateTime)));
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
                result = SubstituteNumber(result, result.Length - Path.GetExtension(result).Length, autoNumberDigits,
                    numberSkip, incrementIfExists);
            }

            return result;
        }

        private string SubstituteNumber(string path, int insertionIndex, int minDigits, int skip,
            bool incrementIfExists)
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