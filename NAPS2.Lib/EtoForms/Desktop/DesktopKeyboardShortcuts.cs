using Eto.Forms;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Desktop;

public class DesktopKeyboardShortcuts
{
    private readonly KeyboardShortcutManager _ksm;
    private readonly Naps2Config _config;

    public DesktopKeyboardShortcuts(KeyboardShortcutManager ksm, Naps2Config config)
    {
        _ksm = ksm;
        _config = config;
    }

    public void Assign(DesktopCommands commands)
    {
        _ksm.Clear();

        // Unconfigurable defaults
        _ksm.Assign("Mod+.", commands.Scan);
        _ksm.Assign("Mod+Up", commands.MoveUp);
        _ksm.Assign("Mod+Left", commands.MoveUp);
        _ksm.Assign("Mod+Down", commands.MoveDown);
        _ksm.Assign("Mod+Right", commands.MoveDown);
        _ksm.Assign("Del", commands.Delete);
        _ksm.Assign("Mod+A", commands.SelectAll);
        _ksm.Assign("Mod+C", commands.Copy);
        _ksm.Assign("Mod+V", commands.Paste);
        _ksm.Assign("Mod+Z", commands.Undo);
        _ksm.Assign(EtoPlatform.Current.IsWinForms ? "Mod+Y" : "Mod+Shift+Z", commands.Redo);

        // Configured defaults

        var ks = _config.Get(c => c.KeyboardShortcuts);

        _ksm.Assign(ks.Settings, commands.Settings);
        _ksm.Assign(ks.About, commands.About);
        _ksm.Assign(ks.BatchScan, commands.BatchScan);
        _ksm.Assign(ks.Clear, commands.ClearAll);
        _ksm.Assign(ks.Delete, commands.Delete);
        if (PlatformCompat.System.CanEmail)
        {
            _ksm.Assign(ks.EmailPDF, commands.EmailPdf);
            _ksm.Assign(ks.EmailPDFAll, commands.EmailAll);
            _ksm.Assign(ks.EmailPDFSelected, commands.EmailSelected);
            _ksm.Assign(ks.EmailSettings, commands.EmailSettings);
        }
        _ksm.Assign(ks.ImageBlackWhite, commands.BlackWhite);
        _ksm.Assign(ks.ImageBrightness, commands.BrightCont);
        _ksm.Assign(ks.ImageContrast, commands.BrightCont);
        _ksm.Assign(ks.ImageCrop, commands.Crop);
        _ksm.Assign(ks.ImageHue, commands.HueSat);
        _ksm.Assign(ks.ImageSaturation, commands.HueSat);
        _ksm.Assign(ks.ImageSharpen, commands.Sharpen);
        _ksm.Assign(ks.ImageDocumentCorrection, commands.DocumentCorrection);
        _ksm.Assign(ks.ImageSplit, commands.Split);
        _ksm.Assign(ks.ImageCombine, commands.Combine);
        _ksm.Assign(ks.ImageReset, commands.ResetImage);
        _ksm.Assign(ks.ImageView, commands.ViewImage);
        _ksm.Assign(ks.Import, commands.Import);
        _ksm.Assign(ks.MoveDown, commands.MoveDown);
        _ksm.Assign(ks.MoveUp, commands.MoveUp);
        _ksm.Assign(ks.NewProfile, commands.NewProfile);
        _ksm.Assign(ks.Ocr, commands.Ocr);
        if (PlatformCompat.System.CanPrint)
        {
            _ksm.Assign(ks.Print, commands.Print);
        }
        _ksm.Assign(ks.Profiles, commands.Profiles);

        _ksm.Assign(ks.ReorderAltDeinterleave, commands.AltDeinterleave);
        _ksm.Assign(ks.ReorderAltInterleave, commands.AltInterleave);
        _ksm.Assign(ks.ReorderDeinterleave, commands.Deinterleave);
        _ksm.Assign(ks.ReorderInterleave, commands.Interleave);
        _ksm.Assign(ks.ReorderReverseAll, commands.ReverseAll);
        _ksm.Assign(ks.ReorderReverseSelected, commands.ReverseSelected);
        _ksm.Assign(ks.RotateCustom, commands.CustomRotate);
        _ksm.Assign(ks.RotateFlip, commands.Flip);
        _ksm.Assign(ks.RotateLeft, commands.RotateLeft);
        _ksm.Assign(ks.RotateRight, commands.RotateRight);
        if (PlatformCompat.System.CombinedPdfAndImageSaving)
        {
            _ksm.Assign(ks.SavePDFAll, commands.SaveAll);
            _ksm.Assign(ks.SavePDFSelected, commands.SaveSelected);
        }
        else
        {
            _ksm.Assign(ks.SaveImages, commands.SaveImages);
            _ksm.Assign(ks.SaveImagesAll, commands.SaveAllImages);
            _ksm.Assign(ks.SaveImagesSelected, commands.SaveSelectedImages);
            _ksm.Assign(ks.SavePDF, commands.SavePdf);
            _ksm.Assign(ks.SavePDFAll, commands.SaveAllPdf);
            _ksm.Assign(ks.SavePDFSelected, commands.SaveSelectedPdf);
        }
        _ksm.Assign(ks.PDFSettings, commands.PdfSettings);
        _ksm.Assign(ks.ImageSettings, commands.ImageSettings);
        _ksm.Assign(ks.ScanDefault, commands.Scan);
        _ksm.Assign(ks.ScannerSharing, commands.ScannerSharing);

        _ksm.Assign(ks.ZoomIn, commands.ZoomIn);
        _ksm.Assign(ks.ZoomOut, commands.ZoomOut);
    }

    public void AssignProfileShortcut(int i, Command command)
    {
        var sh = GetProfileShortcut(i);
        if (string.IsNullOrWhiteSpace(sh) && i <= 11)
        {
            sh = "F" + (i + 1);
        }
        _ksm.Assign(sh, command);
    }

    private string? GetProfileShortcut(int i)
    {
        var ks = _config.Get(c => c.KeyboardShortcuts);
        switch (i)
        {
            case 1:
                return ks.ScanProfile1;
            case 2:
                return ks.ScanProfile2;
            case 3:
                return ks.ScanProfile3;
            case 4:
                return ks.ScanProfile4;
            case 5:
                return ks.ScanProfile5;
            case 6:
                return ks.ScanProfile6;
            case 7:
                return ks.ScanProfile7;
            case 8:
                return ks.ScanProfile8;
            case 9:
                return ks.ScanProfile9;
            case 10:
                return ks.ScanProfile10;
            case 11:
                return ks.ScanProfile11;
            case 12:
                return ks.ScanProfile12;
        }
        return null;
    }

    public bool Perform(Keys keyData)
    {
        return _ksm.Perform(keyData);
    }
}