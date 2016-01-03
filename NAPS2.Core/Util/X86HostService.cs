using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public class X86HostService : IX86HostService
    {
        public void DoWork()
        {
            MessageBox.Show("Hi!");
        }
    }
}
