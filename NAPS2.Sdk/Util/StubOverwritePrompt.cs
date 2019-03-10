using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public class StubOverwritePrompt : OverwritePrompt
    {
        public override DialogResult ConfirmOverwrite(string path) => DialogResult.No;
    }
}