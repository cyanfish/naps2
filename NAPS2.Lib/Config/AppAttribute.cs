using System.Runtime.CompilerServices;

namespace NAPS2.Config;

/// <summary>
/// Annotates a config property as being specified only at the app level and not at the user level. This isn't currently
/// enforced.
/// </summary>
public class AppAttribute : ConfigPropAttribute
{
    public AppAttribute([CallerLineNumber] int line = 0) : base(line)
    {
    }
}