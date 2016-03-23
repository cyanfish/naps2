using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class NotificationManager
    {
        private readonly FormBase parentForm;
        private const int PADDING_X = 25, PADDING_Y = 25;
        private const int SPACING_Y = 20;

        private readonly List<NotifyWidget> slots = new List<NotifyWidget>();

        public NotificationManager(FormBase parentForm)
        {
            this.parentForm = parentForm;
            parentForm.Resize += parentForm_Resize;
        }

        private void parentForm_Resize(object sender, EventArgs e)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].Location = GetPosition(slots[i], i);
                }
            }
        }

        public void Show(NotifyWidget n)
        {
            int slot = FillNextSlot(n);
            n.Location = GetPosition(n, slot);
            n.BringToFront();
            n.HideNotify += (sender, args) => ClearSlot(n);
            n.ShowNotify();
        }

        private void ClearSlot(NotifyWidget n)
        {
            var index = slots.IndexOf(n);
            if (index != -1)
            {
                parentForm.Controls.Remove(n);
                slots[index] = null;
            }
        }

        private int FillNextSlot(NotifyWidget n)
        {
            var index = slots.IndexOf(null);
            if (index == -1)
            {
                index = slots.Count;
                slots.Add(n);
            }
            else
            {
                slots[index] = n;
            }
            parentForm.Controls.Add(n);
            return index;
        }

        private Point GetPosition(NotifyWidget n, int slot)
        {
            return new Point(parentForm.ClientSize.Width - n.Width - PADDING_X,
                parentForm.ClientSize.Height - n.Height - PADDING_Y - (n.Height + SPACING_Y) * slot);
        }
    }
}
