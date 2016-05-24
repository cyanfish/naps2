using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Ocr
{
    public class Language
    {
        public Language(string code, string name)
        {
            Name = name;
            Code = code;
        }

        public string Code { get; private set; }

        public string Name { get; private set; }
    }
}
