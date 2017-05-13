using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NAPS2.WinForms
{
    public partial class ILProfileIcons : Component
    {
        public ILProfileIcons()
        {
            InitializeComponent();
        }

        public ILProfileIcons(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
