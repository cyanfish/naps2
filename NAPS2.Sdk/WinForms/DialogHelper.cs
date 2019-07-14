using System;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public abstract class DialogHelper
    {
        private static DialogHelper _default = new StubDialogHelper();

        public static DialogHelper Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract bool PromptToSavePdfOrImage(string defaultPath, out string savePath);

        public abstract bool PromptToSavePdf(string defaultPath, out string savePath);

        public abstract bool PromptToSaveImage(string defaultPath, out string savePath);
    }
}
