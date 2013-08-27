using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ninject;

namespace NAPS2.WinForms
{
    public class FormBase : Form
    {
        public FormBase(IKernel kernel)
        {
            Kernel = kernel;
        }

        public IKernel Kernel { get; private set; }
    }
}
