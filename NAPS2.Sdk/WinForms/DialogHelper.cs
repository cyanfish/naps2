using System;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public abstract class DialogHelper
    {
        public abstract bool PromptToSavePdfOrImage(string defaultPath, out string savePath);

        public abstract bool PromptToSavePdf(string defaultPath, out string savePath);

        public abstract bool PromptToSaveImage(string defaultPath, out string savePath);
    }
}
