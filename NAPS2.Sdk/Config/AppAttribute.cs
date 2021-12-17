using System.Runtime.CompilerServices;

namespace NAPS2.Config;

public class AppAttribute : ConfigPropAttribute
{
    public AppAttribute([CallerLineNumber] int line = 0) : base(line)
    {
    }
}