using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
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

    public KeyboardShortcutsForm(Naps2Config config, KeyboardShortcutManager ksm) : base(config)
    {
        _ksm = ksm;
        _listBox.DataStore = Shortcuts;
        _listBox.ItemTextBinding = new DelegateBinding<Shortcut, string>(GetLabel);
    }

    private string GetLabel(Shortcut shortcut)
    {
        var keys = _ksm.Parse(Config.Get(shortcut.Accessor));
        if (keys == Keys.None)
        {
            return shortcut.Label;
        }
        // TODO: Better than ToString?
        return string.Format(UiStrings.KeyboardShortcutLabelFormat, shortcut.Label, _ksm.Stringify(keys));
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.KeyboardShortcutsFormTitle;

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);

        LayoutController.Content = L.Column(
            _listBox
        );
    }

    public bool Updated { get; private set; }

    private void Save()
    {
        Updated = true;
    }

    private record Shortcut(string Label, Expression<Func<CommonConfig, string?>> Accessor);
}