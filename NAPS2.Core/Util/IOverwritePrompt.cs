using System.Windows.Forms;

namespace NAPS2.Util
{
    public interface IOverwritePrompt
    {
        DialogResult ConfirmOverwrite(string path);
    }
}