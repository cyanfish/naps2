using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NAPS2.ImportExport;

/// <summary>
/// Class for handling substitution of special values in file paths. For example, "$(YYYY)" can be substituted with the current year.
/// The $(barcode) placeholder uses the barcode that separated the current document.
/// Use Placeholders.All for recommended substitutions. Alternatively, you can use Placeholders.Env or Placeholders.None if you prefer.
/// </summary>
internal abstract class Placeholders
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
    public const string BARCODE = "$(barcode)";
    public const string FULL_DATE = "$(YYYY)-$(MM)-$(DD)";
    public const string FULL_TIME = "$(hh)_$(mm)_$(ss)";

    /// <summary>
    /// Substitutes all the standard placeholders. For example, "$(YYYY)-$(MM)-$(DD) $(hh):$(mm):$(ss)" is substituted with the current date and time. Substitutes environment variables. Handles auto-numbering for multiple files,
    /// using the numeric placeholders ("$(n)", "$(nn)", "$(nnn)", or "$(nnnn)") if specified; otherwise, the number is appended to the file name.
    /// </summary>
    public static DefaultPlaceholders All => new();

    /// <summary>
    /// Substitutes all the standard placeholders. For example, "$(YYYY)-$(MM)-$(DD) $(hh):$(mm):$(ss)" is substituted with the current date and time. Substitutes environment variables.
    /// </summary>
    public static DefaultPlaceholders NonNumeric => new(includeNumeric: false);

    /// <summary>
    /// Substitutes environment variables only. Not recommended if you may be saving multiple files.
    /// </summary>
    public static EnvironmentPlaceholders Env => new();

    /// <summary>
    /// Does not make any changes to the input. Not recommended if you may be saving multiple files.
    /// </summary>
    public static StubPlaceholders None => new();

    /// <summary>
    /// Performs substitutions on the given input string (e.g. a file path).
    /// </summary>
    /// <param name="input">The string to perform substitutions on.</param>
    /// <param name="incrementIfExists">If the input is a file path, whether to use an auto-incrementing file number to make the file name unique.</param>
    /// <param name="numberSkip">The file number will be at least one bigger than this value.</param>
    /// <param name="autoNumberDigits">If the input is a file path, the minimum number of digits in the file number. Only has an effect if the input does not contain a numeric placeholder like $(n) or $(nnn).</param>
    /// <param name="barcodeValue">The value to use for the $(barcode) placeholder.</param>
    /// <returns>The input with substitutions.</returns>
    [return: NotNullIfNotNull("input")]
    public abstract string? Substitute(string? input, bool incrementIfExists = true, int numberSkip = 0,
        int autoNumberDigits = 0, string? barcodeValue = null);

    public class StubPlaceholders : Placeholders
    {
        public override string? Substitute(string? input, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0, string? barcodeValue = null) => input;
    }

    public class EnvironmentPlaceholders : Placeholders
    {
        public override string? Substitute(string? input, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0, string? barcodeValue = null)
        {
            if (input == null) return null;
            return Environment.ExpandEnvironmentVariables(input);
        }
    }

    public class DefaultPlaceholders : Placeholders
    {
        private static readonly Dictionary<string, Func<DateTime, string>> Replacements =
            new()
            {
                { YEAR_4_DIGITS, dateTime => dateTime.ToString("yyyy") },
                { YEAR_2_DIGITS, dateTime => dateTime.ToString("yy") },
                { MONTH_2_DIGITS, dateTime => dateTime.ToString("MM") },
                { DAY_2_DIGITS, dateTime => dateTime.ToString("dd") },
                { HOUR_24_CLOCK, dateTime => dateTime.ToString("HH") },
                { MINUTE_2_DIGITS, dateTime => dateTime.ToString("mm") },
                { SECOND_2_DIGITS, dateTime => dateTime.ToString("ss") },
            };

        private static readonly Regex NumberPlaceholderPattern = new(@"\$\(n+\)");

        private readonly DateTime? _dateTimeOverride;
        private readonly bool _includeNumeric;
        private readonly bool _filterBarcode;

        public DefaultPlaceholders(DateTime? dateTimeOverride = null, bool includeNumeric = true,
            bool filterBarcode = true)
        {
            _dateTimeOverride = dateTimeOverride;
            _includeNumeric = includeNumeric;
            _filterBarcode = filterBarcode;
        }

        /// <summary>
        /// Creates a copy of the DefaultPlaceholders object that will use the specified DateTime for date and time substitutions.
        /// </summary>
        /// <param name="dateTime">The date and time to use.</param>
        /// <returns>The new DefaultPlaceholders object.</returns>
        public DefaultPlaceholders WithDate(DateTime dateTime) => new(dateTime, _includeNumeric, _filterBarcode);

        /// <summary>
        /// Creates a copy that optionally replaces filename-invalid characters in barcode values with underscores.
        /// </summary>
        public DefaultPlaceholders WithBarcodeFilter(bool filterBarcode) =>
            new(_dateTimeOverride, _includeNumeric, filterBarcode);

        [return: NotNullIfNotNull("input")]
        public override string? Substitute(string? input, bool incrementIfExists = true, int numberSkip = 0,
            int autoNumberDigits = 0, string? barcodeValue = null)
        {
            if (input == null)
            {
                return null;
            }

            var dateTime = _dateTimeOverride ?? DateTime.Now;
            // Start with environment variables
            string result = Environment.ExpandEnvironmentVariables(input);
            // Most placeholders don't need a special case
            result = Replacements.Aggregate(result, (current, ph) => current.Replace(ph.Key, ph.Value(dateTime)));
            // One does, however
            if (_includeNumeric)
            {
                var match = NumberPlaceholderPattern.Match(result);
                if (match.Success)
                {
                    result = NumberPlaceholderPattern.Replace(result, "");
                    result = SubstituteNumber(result, match.Index, match.Length - 3, numberSkip, true);
                }
                else if (autoNumberDigits > 0)
                {
                    result = result.Insert(result.Length - Path.GetExtension(result).Length, ".");
                    result = SubstituteNumber(result, result.Length - Path.GetExtension(result).Length,
                        autoNumberDigits,
                        numberSkip, incrementIfExists);
                }
            }

            if (barcodeValue != null)
            {
                string value = _filterBarcode ? SanitizeBarcodeValue(barcodeValue) : barcodeValue;
                result = result.Replace(BARCODE, value, StringComparison.Ordinal);
            }

            return result;
        }

        private static string SanitizeBarcodeValue(string value)
        {
            const string invalidChars = "<>:\"/\\|?*";
            return new string(value.Select(c =>
                char.IsControl(c) || invalidChars.Contains(c) ? '_' : c).ToArray());
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