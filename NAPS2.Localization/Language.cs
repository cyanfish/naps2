using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Localization
{
    public class Language
    {
        public static void Update(string langCode)
        {
            var ctx = new LanguageContext(langCode);
            ctx.Load(Path.Combine(Paths.Root, $@"NAPS2.Core\Lang\po\{langCode}.po"));
            ctx.Translate(Path.Combine(Paths.Root, @"NAPS2.Core\Lang\Resources"), false);
            ctx.Translate(Path.Combine(Paths.Root, @"NAPS2.Core\WinForms"), true);
        }
    }
}