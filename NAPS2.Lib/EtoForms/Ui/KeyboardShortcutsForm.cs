using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.Config.Model;
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
    ];

    private readonly ListBox _listBox = new();
    private readonly TextBox _shortcutText = new();
    private readonly Button _unassign = C.Button(UiStrings.Unassign);
    private readonly Button _restoreDefaults = C.Button(UiStrings.RestoreDefaults);
    private readonly TransactionConfigScope<CommonConfig> _transact;
    private readonly Naps2Config _transactionConfig;

    public KeyboardShortcutsForm(Naps2Config config, KeyboardShortcutManager ksm) : base(config)
    {
        _ksm = ksm;
        _transact = Config.User.BeginTransaction();
        _transactionConfig = Config.WithTransaction(_transact);
        _listBox.DataStore = Shortcuts;
        _listBox.ItemTextBinding = new DelegateBinding<Shortcut, string>(GetLabel);
        _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        ListBox_SelectedIndexChanged(this, EventArgs.Empty);
        _shortcutText.KeyDown += ShortcutText_KeyDown;
        _unassign.Click += Unassign_Click;
        _restoreDefaults.Click += RestoreDefaults_Click;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.KeyboardShortcutsFormTitle;

        LayoutController.Content = L.Column(
            L.Row(
                _listBox.NaturalSize(200, 400).Scale(),
                L.Column(
                    C.Filler(),
                    _shortcutText.Width(150),
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

    private void Unassign_Click(object? sender, EventArgs e)
    {
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null) return;
        _transact.Set(selected.Accessor, "");
        UpdateShortcutText("");
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        foreach (var shortcut in Shortcuts)
        {
            _transact.Remove(shortcut.Accessor);
        }
        _listBox.Invalidate();
        ListBox_SelectedIndexChanged(this, EventArgs.Empty);
    }

    private void ShortcutText_KeyDown(object? sender, KeyEventArgs e)
    {
        // TODO: There's an accessibility issue here of when this textbox is selected, it's impossible to defocus with
        // the keyboard. Maybe a better solution is to keep the textbox disabled until you click the Assign button, then
        // disable it again once a valid key combination has been pressed?
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
        UpdateShortcutText(text);
    }

    private void UpdateShortcutText(string? text)
    {
        _shortcutText.Text = text;
        _listBox.Invalidate();
    }

    private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var selected = (Shortcut?) _listBox.SelectedValue;
        if (selected == null)
        {
            _shortcutText.Text = "";
            _shortcutText.Enabled = false;
            _unassign.Enabled = false;
        }
        else
        {
            _shortcutText.Text = GetKeyString(selected);
            _shortcutText.Enabled = true;
            _unassign.Enabled = true;
        }
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

    public bool Updated { get; private set; }

    private void Save()
    {
        _transact.Commit();
        Updated = true;
    }

    private record Shortcut(string Label, Expression<Func<CommonConfig, string?>> Accessor);
}