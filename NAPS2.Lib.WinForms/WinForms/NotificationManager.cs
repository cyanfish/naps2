using System.Drawing;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Desktop;
using NAPS2.Update;
using wf = System.Windows.Forms;

namespace NAPS2.WinForms;

public class NotificationManager : INotificationManager
{
    private const int PADDING_X = 25, PADDING_Y = 25;
    private const int SPACING_Y = 20;

    private readonly Naps2Config _config;
    private readonly List<NotifyWidgetBase?> _slots = new();
    private wf.Form? _parentForm;

    public NotificationManager(Naps2Config config, DesktopFormProvider desktopFormProvider)
    {
        _config = config;
        desktopFormProvider.DesktopFormChanged += (_, _) =>
        {
            if (_parentForm != null)
            {
                _parentForm.Resize -= parentForm_Resize;
            }
            _parentForm = desktopFormProvider.DesktopForm.ToNative();
            _parentForm.Resize += parentForm_Resize;
        };
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

    public void OperationProgress(OperationProgress opModalProgress, IOperation op)
    {
        Show(new OperationProgressNotifyWidget(opModalProgress, op));
    }

    public void UpdateAvailable(IUpdateChecker updateChecker, UpdateInfo update)
    {
        Show(new UpdateAvailableNotifyWidget(updateChecker, update));
    }

    public void Rebuild()
    {
        var old = _slots.ToList();
        _slots.Clear();
        for (int i = 0; i < old.Count; i++)
        {
            var slot = old[i];
            if (slot != null)
            {
                Show(slot.Clone());
            }
        }
    }

    private void Show(NotifyWidgetBase n)
    {
        if (_config.Get(c => c.DisableSaveNotifications) && n is NotifyWidget)
        {
            return;
        }
            
        Invoker.Current.SafeInvoke(() =>
        {
            int slot = FillNextSlot(n);
            n.Location = GetPosition(n, slot);
            n.Resize += parentForm_Resize;
            n.BringToFront();
            n.HideNotify += (sender, args) => ClearSlot(n);
            n.ShowNotify(); 
        });
    }

    private void parentForm_Resize(object? sender, EventArgs e)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot != null)
            {
                slot.Location = GetPosition(slot, i);
            }
        }
    }

    private void ClearSlot(NotifyWidgetBase n)
    {
        var index = _slots.IndexOf(n);
        if (index != -1)
        {
            _parentForm!.Controls.Remove(n);
            _slots[index] = null;
        }
    }

    private int FillNextSlot(NotifyWidgetBase n)
    {
        var index = _slots.IndexOf(null);
        if (index == -1)
        {
            index = _slots.Count;
            _slots.Add(n);
        }
        else
        {
            _slots[index] = n;
        }
        _parentForm!.Controls.Add(n);
        return index;
    }

    private Point GetPosition(NotifyWidgetBase n, int slot)
    {
        return new Point(_parentForm!.ClientSize.Width - n.Width - PADDING_X,
            _parentForm.ClientSize.Height - n.Height - PADDING_Y - (n.Height + SPACING_Y) * slot);
    }
}