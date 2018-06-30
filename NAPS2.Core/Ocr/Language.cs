﻿namespace NAPS2.Ocr
{
    public class Language
    {
        public Language(string code, string name, bool rtl)
        {
            Name = name;
            Code = code;
            RTL = rtl;
        }

        public string Code { get; }

        public string Name { get; }

        public bool RTL { get; }
    }
}