using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public interface IOverwritePrompt
    {
        DialogResult ConfirmOverwrite(string path);
    }
}
