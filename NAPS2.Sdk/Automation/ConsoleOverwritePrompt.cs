using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsoleOverwritePrompt : OverwritePrompt
    {
        public static bool ForceOverwrite { get; set; }

        private readonly ErrorOutput errorOutput;

        public ConsoleOverwritePrompt(ErrorOutput errorOutput)
        {
            this.errorOutput = errorOutput;
        }

        public override DialogResult ConfirmOverwrite(string path)
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
