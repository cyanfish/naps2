using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    internal partial class FTwainGui : FormBase
    {
        public FTwainGui()
        {
            InitializeComponent();
            RestoreFormState = false;
            // This must be false to avoid cross-process contention over the config file
            SaveFormState = false;
        }
    }
}
