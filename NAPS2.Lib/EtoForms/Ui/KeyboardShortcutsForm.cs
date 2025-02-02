using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.Config.Model;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class KeyboardShortcutsForm : EtoDialogBase
{
    private readonly KeyboardShortcutManager _ksm;
    private static readonly List<Shortcut> Shortcuts =
    [
        new(UiStrings.ScanWithDefaultProfile, c => c.KeyboardShortcuts.ScanDefault),
        new(string.Format(UiStrings.ScanWithProfile, 1), c => c.KeyboardShortcuts.ScanProfile1),
        new(string.Format(UiStrings.ScanWithProfile, 2), c => c.KeyboardShortcuts.ScanProfile2),
        new(string.Format(UiStrings.ScanWithProfile, 3), c => c.KeyboardShortcuts.ScanProfile3),
        new(string.Format(UiStrings.ScanWithProfile, 4), c => c.KeyboardShortcuts.ScanProfile4),
        new(string.Format(UiStrings.ScanWithProfile, 5), c => c.KeyboardShortcuts.ScanProfile5),
        new(string.Format(UiStrings.ScanWithProfile, 6), c => c.KeyboardShortcuts.ScanProfile6),
        new(string.Format(UiStrings.ScanWithProfile, 7), c => c.KeyboardShortcuts.ScanProfile7),
        new(string.Format(UiStrings.ScanWithProfile, 8), c => c.KeyboardShortcuts.ScanProfile8),
        new(string.Format(UiStrings.ScanWithProfile, 9), c => c.KeyboardShortcuts.ScanProfile9),
        new(string.Format(UiStrings.ScanWithProfile, 10), c => c.KeyboardShortcuts.ScanProfile10),
        new(string.Format(UiStrings.ScanWithProfile, 11), c => c.KeyboardShortcuts.ScanProfile11),
        new(string.Format(UiStrings.ScanWithProfile, 12), c => c.KeyboardShortcuts.ScanProfile12),
        new(UiStrings.ScanWithNewProfile, c => c.KeyboardShortcuts.NewProfile),
        new(UiStrings.BatchScan, c => c.KeyboardShortcuts.BatchScan),

        new(UiStrings.Profiles, c => c.KeyboardShortcuts.Profiles),
        new(UiStrings.ScannerSharing, c => c.KeyboardShortcuts.ScannerSharing),
        new(UiStrings.Ocr, c => c.KeyboardShortcuts.Ocr),
        new(UiStrings.Import, c => c.KeyboardShortcuts.Import),

        new(UiStrings.SavePdf, c => c.KeyboardShortcuts.SavePDF),
        new(UiStrings.SaveAllAsPdf, c => c.KeyboardShortcuts.SavePDFAll),
        new(UiStrings.SaveSelectedAsPdf, c => c.KeyboardShortcuts.SavePDFSelected),
        new(UiStrings.PdfSettings, c => c.KeyboardShortcuts.PDFSettings),

        new(UiStrings.SaveImages, c => c.KeyboardShortcuts.SaveImages),
        new(UiStrings.SaveAllAsImages, c => c.KeyboardShortcuts.SaveImagesAll),
        new(UiStrings.SaveSelectedAsImages, c => c.KeyboardShortcuts.SaveImagesSelected),
        new(UiStrings.ImageSettings, c => c.KeyboardShortcuts.ImageSettings),

        new(UiStrings.EmailPdf, c => c.KeyboardShortcuts.EmailPDF),
        new(UiStrings.EmailAllAsPdf, c => c.KeyboardShortcuts.EmailPDFAll),
        new(UiStrings.EmailSelectedAsPdf, c => c.KeyboardShortcuts.EmailPDFSelected),
        new(UiStrings.EmailSettings, c => c.KeyboardShortcuts.EmailSettings),

        new(UiStrings.Print, c => c.KeyboardShortcuts.Print),

        new(UiStrings.View, c => c.KeyboardShortcuts.ImageView),
        new(UiStrings.BlackAndWhite, c => c.KeyboardShortcuts.ImageView),
        new(UiStrings.BrightnessContrast, c => c.KeyboardShortcuts.ImageBrightness),
        new(UiStrings.Crop, c => c.KeyboardShortcuts.ImageCrop),
        new(UiStrings.HueSaturation, c => c.KeyboardShortcuts.ImageHue),
        new(UiStrings.Sharpen, c => c.KeyboardShortcuts.ImageSharpen),
        new(UiStrings.DocumentCorrection, c => c.KeyboardShortcuts.ImageDocumentCorrection),
        new(UiStrings.Split, c => c.KeyboardShortcuts.ImageSplit),
        new(UiStrings.Combine, c => c.KeyboardShortcuts.ImageCombine),
        new(UiStrings.Reset, c => c.KeyboardShortcuts.ImageReset),

        new(UiStrings.RotateLeft, c => c.KeyboardShortcuts.RotateLeft),
        new(UiStrings.RotateRight, c => c.KeyboardShortcuts.RotateRight),
        new(UiStrings.Flip, c => c.KeyboardShortcuts.RotateFlip),
        new(UiStrings.CustomRotation, c => c.KeyboardShortcuts.RotateCustom),

        new(UiStrings.MoveUp, c => c.KeyboardShortcuts.MoveUp),
        new(UiStrings.MoveDown, c => c.KeyboardShortcuts.MoveDown),

        new(UiStrings.Interleave, c => c.KeyboardShortcuts.ReorderInterleave),
        new(UiStrings.Deinterleave, c => c.KeyboardShortcuts.ReorderDeinterleave),
        new(UiStrings.AltInterleave, c => c.KeyboardShortcuts.ReorderAltInterleave),
        new(UiStrings.AltDeinterleave, c => c.KeyboardShortcuts.ReorderAltDeinterleave),
        new(UiStrings.ReverseAll, c => c.KeyboardShortcuts.ReorderReverseAll),
        new(UiStrings.ReverseSelected, c => c.KeyboardShortcuts.ReorderReverseSelected),

        new(UiStrings.Delete, c => c.KeyboardShortcuts.Delete),
        new(UiStrings.Clear, c => c.KeyboardShortcuts.Clear),

        new(UiStrings.Settings, c => c.KeyboardShortcuts.Settings),
        new(UiStrings.About, c => c.KeyboardShortcuts.About),

        new(UiStrings.ZoomIn, c => c.KeyboardShortcuts.ZoomIn),
        new(UiStrings.ZoomOut, c => c.KeyboardShortcuts.ZoomOut),
    ];

    private readonly DesktopFormProvider _desktopFormProvider;

    private readonly ListBox _listBox = new();
    private readonly TextBox _shortcutText = new();
    private readonly Button _assign = C.Button(UiStrings.Assign);
    private readonly Button _unassign = C.Button(UiStrings.Unassign);
    private readonly Button _restoreDefaults = C.Button(UiStrings.RestoreDefaults);

    private readonly TransactionConfigScope<CommonConfig> _transact;
    private readonly Naps2Config _transactionConfig;

    public KeyboardShortcutsForm(Naps2Config config, KeyboardShortcutManager ksm,
        DesktopFormProvider desktopFormProvider) : base(config)
    {
        _ksm = ksm;
        _desktopFormProvider = desktopFormProvider;
        _transact = Config.User.BeginTransaction();
        _transactionConfig = Config.WithTransaction(_transact);
        _listBox.DataStore = Shortcuts;
        _listBox.ItemTextBinding = new DelegateBinding<Shortcut, string>(GetLabel);
        _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        _shortcutText.KeyDown += ShortcutText_KeyDown;
        _assign.Click += Assign_Click;
        _unassign.Click += Unassign_Click;
        _restoreDefaults.Click += RestoreDefaults_Click;
        UpdateUi();
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.KeyboardShortcutsFormTitle;
        IconName = "keyboard_small";

        LayoutController.Content = L.Column(
            L.Row(
                _listBox.NaturalSize(300, 500).Scale(),
                L.Column(
                    C.Filler(),
                    _shortcutText.Width(150),
                    _assign,
                    _unassign,
                    C.Filler()
                )
            ).Scale(),
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(C.OkButton(this, Save), C.CancelButton(this))
            )
        );
    }

    private void Assign_Click(object? sender, EventArgs e)
    {
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null) return;
        _shortcutText.Enabled = true;
        _shortcutText.Focus();
    }

    private void Unassign_Click(object? sender, EventArgs e)
    {
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null) return;
        _transact.Set(selected.Accessor, "");
        UpdateUi();
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        foreach (var shortcut in Shortcuts)
        {
            _transact.Remove(shortcut.Accessor);
        }
        UpdateUi();
    }

    private void ShortcutText_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!_shortcutText.Enabled) return;

        e.Handled = true;
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null) return;
        if (e.Key is Keys.LeftControl or Keys.LeftAlt or Keys.LeftShift or Keys.LeftApplication
            or Keys.RightControl or Keys.RightAlt or Keys.RightShift or Keys.RightApplication)
        {
            return;
        }
        var text = _ksm.Stringify(e.KeyData);
        _transact.Set(selected.Accessor, text);
        UpdateUi();
    }

    private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateUi();
    }

    private void UpdateUi()
    {
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null)
        {
            _shortcutText.Text = "";
            _shortcutText.Enabled = false;
            _assign.Enabled = false;
            _unassign.Enabled = false;
        }
        else
        {
            _shortcutText.Text = GetKeyString(selected);
            _shortcutText.Enabled = false;
            _assign.Enabled = true;
            _unassign.Enabled = _shortcutText.Text != "";
        }
        _listBox.Invalidate();
    }

    private string GetKeyString(Shortcut shortcut)
    {
        var keys = _ksm.Parse(_transactionConfig.Get(shortcut.Accessor));
        return _ksm.Stringify(keys) ?? "";
    }

    private string GetLabel(Shortcut shortcut)
    {
        var keys = _ksm.Parse(_transactionConfig.Get(shortcut.Accessor));
        if (keys == Keys.None)
        {
            return shortcut.Label;
        }
        return string.Format(UiStrings.KeyboardShortcutLabelFormat, shortcut.Label, _ksm.Stringify(keys));
    }

    private void Save()
    {
        _transact.Commit();
        _desktopFormProvider.DesktopForm.AssignKeyboardShortcuts();
    }

    private record Shortcut(string Label, Expression<Func<CommonConfig, string?>> Accessor);
}