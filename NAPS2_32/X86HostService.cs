using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;

namespace NAPS2_32
{
    public class X86HostService : IX86HostService
    {
        public void DoWork()
        {
            MessageBox.Show("Hi!");
        }
    }
}
