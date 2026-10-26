using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;

namespace NAPS2.ImportExport;

/// <summary>
/// Computes file names (and other strings like PDF metadata) from user-specified names containing placeholders such
/// as "$(YYYY)" for the current year or "$(nnnn)" for an auto-incrementing number.
///
/// Names can be reserved so that concurrent saves don't choose the same file, even if the first file hasn't been
/// written yet (e.g. because OCR is still running). A reservation should be held until the file is written.
/// </summary>
public class NameManager
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
    public const string FULL_DATE = "$(YYYY)-$(MM)-$(DD)";
    public const string FULL_TIME = "$(hh)_$(mm)_$(ss)";

    private static readonly Dictionary<string, Func<DateTime, string>> DateReplacements = new()
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

    private readonly Lock _lock = new();
    private readonly Dictionary<string, int> _reserved;

    public NameManager()
    {
        var comparer = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        _reserved = new Dictionary<string, int>(comparer);
    }

    /// <summary>
    /// Computes the name for the given input without reserving it.
    /// </summary>
    [return: NotNullIfNotNull(nameof(input))]
    public string? Get(string? input, NameContext context)
    {
        if (input == null)
        {
            return null;
        }
        return Resolve(input, context, false, out _);
    }

    /// <summary>
    /// Computes the name for the given input and reserves it so that no other caller is given the same name until the
    /// reservation is disposed. The reservation should be held until the file has been written.
    /// </summary>
    public Reservation Reserve(string input, NameContext context)
    {
        var name = Resolve(input, context, true, out var reservationKey);
        return new Reservation(this, name, reservationKey);
    }

    private string Resolve(string input, NameContext context, bool reserve, out string? reservationKey)
    {
        reservationKey = null;
        var result = Environment.ExpandEnvironmentVariables(input);
        foreach (var (placeholder, format) in DateReplacements)
        {
            result = result.Replace(placeholder, format(context.DateTime));
        }
        if (context.Numbering == Numbering.None)
        {
            return result;
        }

        var numberMatch = NumberPlaceholderPattern.Match(result);
        bool numberOnConflict = context.Numbering == Numbering.OnConflict && context.IsFilePath;
        if (!numberMatch.Success && context.Numbering != Numbering.Always && !numberOnConflict)
        {
            if (reserve && context.IsFilePath)
            {
                reservationKey = AddReservation(result);
            }
            return result;
        }

        var withNumber = numberMatch.Success
            ? NumberFromPlaceholder(result, numberMatch)
            : NumberBeforeExtension(result, context.BatchCount);
        if (!context.IsFilePath)
        {
            return withNumber((context.BatchIndex ?? 0) + 1);
        }

        lock (_lock)
        {
            if (numberOnConflict && !numberMatch.Success && !IsTaken(result, context))
            {
                if (reserve)
                {
                    reservationKey = AddReservation(result);
                }
                return result;
            }
            int number = context.BatchIndex ?? 0;
            string candidate;
            do
            {
                candidate = withNumber(++number);
            } while (IsTaken(candidate, context));
            if (reserve)
            {
                reservationKey = AddReservation(candidate);
            }
            return candidate;
        }
    }

    private static Func<int, string> NumberFromPlaceholder(string name, Match numberMatch)
    {
        var prefix = name.Substring(0, numberMatch.Index);
        // Any additional number placeholders are removed
        var suffix = NumberPlaceholderPattern.Replace(name.Substring(numberMatch.Index + numberMatch.Length), "");
        int digits = numberMatch.Length - "$()".Length;
        return number => prefix + number.ToString("D" + digits) + suffix;
    }

    private static Func<int, string> NumberBeforeExtension(string name, int? batchCount)
    {
        var extension = Path.GetExtension(name);
        var prefix = name.Substring(0, name.Length - extension.Length) + ".";
        int digits = batchCount is > 0 ? (int) Math.Floor(Math.Log10(batchCount.Value)) + 1 : 1;
        return number => prefix + number.ToString("D" + digits) + extension;
    }

    private bool IsTaken(string path, NameContext context)
    {
        if (_reserved.ContainsKey(GetReservationKey(path)))
        {
            return true;
        }
        return !context.Overwrite && File.Exists(path);
    }

    private string AddReservation(string path)
    {
        var key = GetReservationKey(path);
        lock (_lock)
        {
            _reserved[key] = _reserved.GetValueOrDefault(key) + 1;
        }
        return key;
    }

    private void RemoveReservation(string key)
    {
        lock (_lock)
        {
            if (_reserved.TryGetValue(key, out var count))
            {
                if (count > 1)
                {
                    _reserved[key] = count - 1;
                }
                else
                {
                    _reserved.Remove(key);
                }
            }
        }
    }

    private static string GetReservationKey(string path) => Path.GetFullPath(path);

    /// <summary>
    /// A reserved name. Disposing the reservation makes the name available again (though if the file has been written
    /// it will still be skipped by numbering).
    /// </summary>
    public sealed class Reservation : IDisposable
    {
        private readonly NameManager _nameManager;
        private string? _key;

        internal Reservation(NameManager nameManager, string name, string? key)
        {
            _nameManager = nameManager;
            Name = name;
            _key = key;
        }

        public string Name { get; }

        public void Dispose()
        {
            if (_key != null)
            {
                _nameManager.RemoveReservation(_key);
                _key = null;
            }
        }
    }
}
