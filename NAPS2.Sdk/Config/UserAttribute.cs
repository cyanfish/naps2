using System.Runtime.CompilerServices;

namespace NAPS2.Config
{
    public class UserAttribute : ConfigPropAttribute
    {
        public UserAttribute([CallerLineNumber] int line = 0) : base(line)
        {
        }
    }
}
