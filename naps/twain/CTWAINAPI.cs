using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NAPS.twain
{
    public class CTWAINAPI
    {
        Twain tw;
        Form parent;
        CScanSettings settings;

        public static string SelectDeviceUI()
        {
            Twain tw = new Twain();
            if (!tw.Init(Application.OpenForms[0].Handle))
            {
                throw new Exceptions.ENoScannerFound();
            }
            tw.Select();
            return tw.GetCurrentName();
        }

        public CTWAINAPI(CScanSettings settings)
        {
            this.settings = settings;
        }

        public CTWAINAPI(CScanSettings settings, Form pForm)
        {
            parent = pForm;
            tw = new Twain();
            this.settings = settings;
            if (!tw.Init(parent.Handle))
            {
                throw new Exceptions.EScannerNotFound();
            }
            if (!tw.SelectByName(settings.DeviceID))
            {
                throw new Exceptions.EScannerNotFound();
            }
        }

        public List<CScannedImage> Scan()
        {
            FTwainGui fg = new FTwainGui(settings);
            fg.TwainIface = tw;
            fg.ShowDialog(parent);
            return fg.Bitmaps;
        }
    }
}
