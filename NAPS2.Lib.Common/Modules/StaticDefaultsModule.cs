using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Operation;
using NAPS2.Util;
using NAPS2.WinForms;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Lib.Modules
{
    public class StaticDefaultsModule : NinjectModule
    {
        public override void Load()
        {
            OperationProgress.Default = Kernel.Get<OperationProgress>();
            ErrorOutput.Default = Kernel.Get<MessageBoxErrorOutput>();
            DialogHelper.Default = Kernel.Get<DialogHelper>();
            OverwritePrompt.Default = Kernel.Get<OverwritePrompt>();
        }
    }
}
