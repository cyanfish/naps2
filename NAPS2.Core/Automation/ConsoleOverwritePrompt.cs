using NAPS2.Lang.ConsoleResources;
using NAPS2.Util;
using System.Windows.Forms;

namespace NAPS2.Automation
{
    public class ConsoleOverwritePrompt : IOverwritePrompt
    {
        public static bool ForceOverwrite { get; set; }

        private readonly IErrorOutput errorOutput;

        public ConsoleOverwritePrompt(IErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
        }

        public DialogResult ConfirmOverwrite(string path)
        {
            if (ForceOverwrite)
            {
                return DialogResult.Yes;
            }
            else
            {
                errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
                return DialogResult.No;
            }
        }
    }
}