using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NAPS
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
