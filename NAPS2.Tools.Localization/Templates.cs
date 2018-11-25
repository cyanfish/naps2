using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Localization
{
    public class Templates
    {
        public static void Update()
        {
            var ctx = new TemplatesContext();
            ctx.Load(Path.Combine(Paths.Root, @"NAPS2.Core\Lang\Resources"), false);
            ctx.Load(Path.Combine(Paths.Root, @"NAPS2.Core\WinForms"), true);
            ctx.Save(Path.Combine(Paths.Root, @"NAPS2.Core\Lang\po\templates.pot"));
        }
    }
}