using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Util
{
    public interface IErrorOutput
    {
        void DisplayError(string errorMessage);
    }
}
