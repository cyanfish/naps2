using System.Globalization;
using System.Xml.Linq;

namespace NAPS2.Escl;

internal class ParseHelper
{
    public static T MaybeParseEnum<T>(XElement? element, T defaultValue) where T : struct =>
        Enum.TryParse<T>(element?.Value, out var value) ? value : defaultValue;

    public static int? MaybeParseInt(XElement? element) =>
        int.TryParse(element?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
}