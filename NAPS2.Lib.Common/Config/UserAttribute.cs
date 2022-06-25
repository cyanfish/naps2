using System.Runtime.CompilerServices;

namespace NAPS2.Config;

/// <summary>
/// Annotates a config property as being specified only at the user level and not at the app level. This isn't currently
/// enforced.
/// </summary>
public class UserAttribute : ConfigPropAttribute
{
    public UserAttribute([CallerLineNumber] int line = 0) : base(line)
    {
    }
}