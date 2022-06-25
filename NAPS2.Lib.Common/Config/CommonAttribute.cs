using System.Runtime.CompilerServices;

namespace NAPS2.Config;

/// <summary>
/// Annotates a config property as being specified at either the user or app level. This isn't currently enforced.
/// </summary>
public class CommonAttribute : ConfigPropAttribute
{
    public CommonAttribute([CallerLineNumber] int line = 0) : base(line)
    {
    }
}