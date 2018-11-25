using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    public interface IFormFactory
    {
        T Create<T>() where T : FormBase;
    }
}
