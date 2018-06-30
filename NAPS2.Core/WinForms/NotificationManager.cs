using NAPS2.Config;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NAPS2.WinForms
{
    public class NotificationManager : ISaveNotify
    {
        private const int PADDING_X = 25, PADDING_Y = 25;
        private const int SPACING_Y = 20;

        private readonly FormBase ParentForm;
        private readonly AppConfigManager appConfigManager;

        private readonly List<NotifyWidget> slots = new List<NotifyWidget>();

        public NotificationManager(FormBase ParentForm, AppConfigManager appConfigManager)
        {
            this.ParentForm = ParentForm;
            this.appConfigManager = appConfigManager;
            ParentForm.Resize += ParentForm_Resize;
        }

        public void PdfSaved(string path)
        {
            Show(new PdfSavedNotifyWidget(path));
        }

        public void ImagesSaved(int imageCount, string path)
        {
            if (imageCount == 1)
            {
                Show(new OneImageSavedNotifyWidget(path));
            }
            else if (imageCount > 1)
            {
                Show(new ImagesSavedNotifyWidget(imageCount, path));
            }
        }

        public void DonatePrompt()
        {
            Show(new DonatePromptNotifyWidget());
        }

        private void Show(NotifyWidget n)
        {
            if (appConfigManager.Config.DisableSaveNotifications)
            {
                return;
            }

            int slot = FillNextSlot(n);
            n.Location = GetPosition(n, slot);
            n.BringToFront();
            n.HideNotify += (sender, args) => ClearSlot(n);
            n.ShowNotify();
        }

        private void ParentForm_Resize(object sender, EventArgs e)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].Location = GetPosition(slots[i], i);
                }
            }
        }

        private void ClearSlot(NotifyWidget n)
        {
            var index = slots.IndexOf(n);
            if (index != -1)
            {
                ParentForm.Controls.Remove(n);
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
            ParentForm.Controls.Add(n);
            return index;
        }

        private Point GetPosition(NotifyWidget n, int slot)
        {
            return new Point(ParentForm.ClientSize.Width - n.Width - PADDING_X,
                ParentForm.ClientSize.Height - n.Height - PADDING_Y - ((n.Height + SPACING_Y) * slot));
        }
    }
}