using NAPS2.Scan;

namespace NAPS2.EtoForms.Widgets;

public class EnumDropDownWidget<T> : DropDownWidget<T> where T : struct, Enum
{
    public static IEnumerable<T> DefaultItems => (T[]) Enum.GetValues(typeof(T));

    public EnumDropDownWidget()
    {
        Format = x => x.Description();
        Items = DefaultItems;
    }
}