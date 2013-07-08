using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2
{
    public interface IErrorOutput
    {
        void DisplayError(string errorMessage);
    }
}
