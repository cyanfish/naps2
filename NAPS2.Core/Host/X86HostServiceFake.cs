using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;

namespace NAPS2.Host
{
    /// <summary>
    /// Non-WCF implementation of IX86HostService for when the NAPS2 process is already x86.
    /// </summary>
    public class X86HostServiceFake : IX86HostService
    {
        private readonly TwainWrapper twainWrapper;

        public X86HostServiceFake(TwainWrapper twainWrapper)
        {
            this.twainWrapper = twainWrapper;
        }

        public void DoWork()
        {
            MessageBox.Show("Hi from " + Process.GetCurrentProcess().Id + "!");
        }

        public ScanDevice TwainPromptForDevice(IntPtr hwnd)
        {
            return twainWrapper.PromptForDevice();
        }

        public List<ScannedImage> TwainScan(IntPtr hwnd, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams)
        {
            var dialogParent = Application.OpenForms.Cast<Form>().Single(x => x.Handle == hwnd);
            return twainWrapper.Scan(dialogParent, scanDevice, scanProfile, scanParams);
        }
    }
}
