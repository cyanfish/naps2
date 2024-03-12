using Eto.Forms;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.EtoForms.Ui;

public class DesktopCommands
{
    private readonly DesktopController _desktopController;
    private readonly DesktopScanController _desktopScanController;
    private readonly IDesktopSubFormController _desktopSubFormController;
    private readonly UiImageList _imageList;
    private readonly ImageListActions _imageListActions;
    private readonly ThumbnailController _thumbnailController;
    private readonly IIconProvider _iconProvider;
    private readonly Naps2Config _config;

    public DesktopCommands(DesktopController desktopController, DesktopScanController desktopScanController,
        IDesktopSubFormController desktopSubFormController, UiImageList imageList, ImageListActions imageListActions,
        ThumbnailController thumbnailController, IIconProvider iconProvider, Naps2Config config)
    {
        _desktopController = desktopController;
        _desktopScanController = desktopScanController;
        _desktopSubFormController = desktopSubFormController;
        _imageList = imageList;
        _imageListActions = imageListActions;
        _thumbnailController = thumbnailController;
        _iconProvider = iconProvider;
        _config = config;

        var hiddenButtons = config.Get(c => c.HiddenButtons);

        Scan = new ActionCommand(desktopScanController.ScanDefault)
        {
            Text = UiStrings.Scan,
            Image = iconProvider.GetIcon("control_play_blue"),
            Shortcut = Application.Instance.CommonModifier | Keys.Period
        };
        NewProfile = new ActionCommand(desktopScanController.ScanWithNewProfile)
        {
            Text = UiStrings.NewProfile,
            Image = iconProvider.GetIcon("add_small"),
            Shortcut = Application.Instance.CommonModifier | Keys.N
        };
        BatchScan = new ActionCommand(desktopSubFormController.ShowBatchScanForm)
        {
            Text = UiStrings.BatchScan,
            Image = iconProvider.GetIcon("application_cascade"),
            Shortcut = Application.Instance.CommonModifier | Keys.B
        };
        ScannerSharing = new ActionCommand(desktopSubFormController.ShowScannerSharingForm)
        {
            Text = UiStrings.ScannerSharing,
            Image = iconProvider.GetIcon("wireless16"),
            Shortcut = Application.Instance.CommonModifier | Keys.G
        };
        Profiles = new ActionCommand(desktopSubFormController.ShowProfilesForm)
        {
            Text = UiStrings.Profiles,
            Image = iconProvider.GetIcon("blueprints"),
            Shortcut = Application.Instance.CommonModifier | Keys.L
        };
        Ocr = new ActionCommand(desktopSubFormController.ShowOcrForm)
        {
            Text = UiStrings.Ocr,
            Image = iconProvider.GetIcon("text"),
            Shortcut = Application.Instance.CommonModifier | Keys.Alt | Keys.O
        };
        Import = new ActionCommand(desktopController.Import)
        {
            Text = UiStrings.Import,
            Image = iconProvider.GetIcon("folder_picture"),
            Shortcut = Application.Instance.CommonModifier | Keys.O
        };
        // Squeeze Icon Shortcut?
        Squeeze = new ActionCommand(desktopController.Squeeze)
        {
            Text = UiStrings.Squeeze,
            Image = iconProvider.GetIcon("inbox_out-white")
        };
        SqueezeAll = new ActionCommand(_imageListActions.SelectAll)
        {
            Text = UiStrings.SqueezeAll
        };
        SqueezeSelected = new ActionCommand(_imageListActions.SelectAll)
        {
            Text = UiStrings.SqueezeSelected
        };
        SqueezeSettings = new ActionCommand(desktopSubFormController.ShowSqueezeSettingsForm)
        {
            Text = UiStrings.SqueezeSettings,
            Image = iconProvider.GetIcon("cog_small")
        };
        SaveAll = new ActionCommand(_imageListActions.SaveAllAsPdfOrImages)
        {
            Text = UiStrings.SaveAll,
            Image = iconProvider.GetIcon("diskette"),
            Shortcut = Application.Instance.CommonModifier | Keys.S
        };
        SaveSelected = new ActionCommand(_imageListActions.SaveSelectedAsPdfOrImages)
        {
            Text = UiStrings.SaveSelected,
            Image = iconProvider.GetIcon("diskette"),
            Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S
        };
        SavePdf = new ActionCommand(desktopController.SavePdf)
        {
            Text = UiStrings.SavePdf,
            Image = iconProvider.GetIcon("file_extension_pdf")
        };
        SaveAllPdf = new ActionCommand(imageListActions.SaveAllAsPdf)
        {
            Text = UiStrings.SaveAllAsPdf
        };
        SaveSelectedPdf = new ActionCommand(imageListActions.SaveSelectedAsPdf)
        {
            Text = UiStrings.SaveSelectedAsPdf
        };
        PdfSettings = new ActionCommand(desktopSubFormController.ShowPdfSettingsForm)
        {
            Text = UiStrings.PdfSettings,
            Shortcut = Application.Instance.CommonModifier | Keys.Alt | Keys.P
        };
        SaveImages = new ActionCommand(desktopController.SaveImages)
        {
            Text = UiStrings.SaveImages,
            Image = iconProvider.GetIcon("pictures")
        };
        SaveAllImages = new ActionCommand(imageListActions.SaveAllAsImages)
        {
            Text = UiStrings.SaveAllAsImages
        };
        SaveSelectedImages = new ActionCommand(imageListActions.SaveSelectedAsImages)
        {
            Text = UiStrings.SaveSelectedAsImages
        };
        ImageSettings = new ActionCommand(desktopSubFormController.ShowImageSettingsForm)
        {
            Text = UiStrings.ImageSettings,
            Shortcut = Application.Instance.CommonModifier | Keys.Alt | Keys.I
        };
        EmailPdf = new ActionCommand(desktopController.EmailPdf)
        {
            Text = UiStrings.EmailPdf,
            Image = iconProvider.GetIcon("email_attach")
        };
        EmailAll = new ActionCommand(imageListActions.EmailAllAsPdf)
        {
            Text = UiStrings.EmailAll,
            Shortcut = Application.Instance.CommonModifier | Keys.E
        };
        EmailSelected = new ActionCommand(imageListActions.EmailSelectedAsPdf)
        {
            Text = UiStrings.EmailSelected,
            Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.E
        };
        EmailSettings = new ActionCommand(desktopSubFormController.ShowEmailSettingsForm)
        {
            Text = UiStrings.EmailSettings,
            Shortcut = Application.Instance.CommonModifier | Keys.Alt | Keys.E
        };
        Print = new ActionCommand(desktopController.Print)
        {
            Text = UiStrings.Print,
            Image = iconProvider.GetIcon("printer"),
            Shortcut = Application.Instance.CommonModifier | Keys.P
        };
        ImageMenu = new ActionCommand
        {
            Text = UiStrings.Image,
            Image = iconProvider.GetIcon("picture_edit")
        };
        ViewImage = new ActionCommand(desktopSubFormController.ShowViewerForm)
        {
            Text = UiStrings.View,
            Image = iconProvider.GetIcon("zoom_small")
        };
        Crop = new ActionCommand(desktopSubFormController.ShowCropForm)
        {
            Text = UiStrings.Crop,
            Image = iconProvider.GetIcon("transform_crop")
        };
        BrightCont = new ActionCommand(desktopSubFormController.ShowBrightnessContrastForm)
        {
            Text = UiStrings.BrightnessContrast,
            Image = iconProvider.GetIcon("contrast_with_sun")
        };
        HueSat = new ActionCommand(desktopSubFormController.ShowHueSaturationForm)
        {
            Text = UiStrings.HueSaturation,
            Image = iconProvider.GetIcon("color_management")
        };
        BlackWhite = new ActionCommand(desktopSubFormController.ShowBlackWhiteForm)
        {
            Text = UiStrings.BlackAndWhite,
            Image = iconProvider.GetIcon("contrast_high")
        };
        Sharpen = new ActionCommand(desktopSubFormController.ShowSharpenForm)
        {
            Text = UiStrings.Sharpen,
            Image = iconProvider.GetIcon("sharpen")
        };
        // TODO: Make this an image form with options
        DocumentCorrection = new ActionCommand(desktopController.RunDocumentCorrection)
        {
            Text = UiStrings.DocumentCorrection
        };
        ResetImage = new ActionCommand(desktopController.ResetImage)
        {
            Text = UiStrings.Reset
        };
        RotateMenu = new ActionCommand
        {
            Text = UiStrings.Rotate,
            Image = iconProvider.GetIcon("arrow_rotate_anticlockwise")
        };
        RotateLeft = new ActionCommand(imageListActions.RotateLeft)
        {
            Text = UiStrings.RotateLeft,
            Image = iconProvider.GetIcon("arrow_rotate_anticlockwise_small")
        };
        RotateRight = new ActionCommand(imageListActions.RotateRight)
        {
            Text = UiStrings.RotateRight,
            Image = iconProvider.GetIcon("arrow_rotate_clockwise_small")
        };
        Flip = new ActionCommand(imageListActions.Flip)
        {
            Text = UiStrings.Flip,
            Image = iconProvider.GetIcon("arrow_switch_small")
        };
        Deskew = new ActionCommand(imageListActions.Deskew)
        {
            Text = UiStrings.Deskew
        };
        CustomRotate = new ActionCommand(desktopSubFormController.ShowRotateForm)
        {
            Text = UiStrings.CustomRotation
        };
        MoveUp = new ActionCommand(imageListActions.MoveUp)
        {
            Text = UiStrings.MoveUp,
            Image = iconProvider.GetIcon("arrow_up_small")
        };
        MoveDown = new ActionCommand(imageListActions.MoveDown)
        {
            Text = UiStrings.MoveDown,
            Image = iconProvider.GetIcon("arrow_down_small")
        };
        ReorderMenu = new ActionCommand
        {
            Text = UiStrings.Reorder,
            Image = iconProvider.GetIcon("arrow_refresh")
        };
        Interleave = new ActionCommand(imageListActions.Interleave)
        {
            Text = UiStrings.Interleave
        };
        Deinterleave = new ActionCommand(imageListActions.Deinterleave)
        {
            Text = UiStrings.Deinterleave
        };
        AltInterleave = new ActionCommand(imageListActions.AltInterleave)
        {
            Text = UiStrings.AltInterleave
        };
        AltDeinterleave = new ActionCommand(imageListActions.AltDeinterleave)
        {
            Text = UiStrings.AltDeinterleave
        };
        ReverseMenu = new ActionCommand
        {
            Text = UiStrings.Reverse
        };
        ReverseAll = new ActionCommand(imageListActions.ReverseAll)
        {
            Text = UiStrings.ReverseAll
        };
        ReverseSelected = new ActionCommand(imageListActions.ReverseSelected)
        {
            Text = UiStrings.ReverseSelected
        };
        Delete = new ActionCommand(desktopController.Delete)
        {
            Text = UiStrings.Delete,
            Image = iconProvider.GetIcon("cross"),
            Shortcut = Keys.Delete
        };
        ClearAll = new ActionCommand(desktopController.Clear)
        {
            ToolBarText = UiStrings.Clear,
            MenuText = UiStrings.ClearAll,
            Image = iconProvider.GetIcon("cancel"),
            Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.Delete
        };
        LanguageMenu = new ActionCommand
        {
            Text = UiStrings.Language,
            Image = iconProvider.GetIcon("world")
        };
        Settings = new ActionCommand(desktopSubFormController.ShowSettingsForm)
        {
            Text = UiStrings.Settings,
            Image = iconProvider.GetIcon(hiddenButtons.HasFlag(ToolbarButtons.About) ? "cog" : "cog_small")
        };
        About = new ActionCommand(desktopSubFormController.ShowAboutForm)
        {
            Text = UiStrings.About,
            Image = iconProvider.GetIcon(hiddenButtons.HasFlag(ToolbarButtons.Settings) ? "information" : "information_small")
        };
        ZoomIn = new ActionCommand(() => thumbnailController.StepSize(1))
        {
            Text = UiStrings.ZoomIn,
            Image = iconProvider.GetIcon("zoom_in")
        };
        ZoomOut = new ActionCommand(() => thumbnailController.StepSize(-1))
        {
            Text = UiStrings.ZoomOut,
            Image = iconProvider.GetIcon("zoom_out")
        };
        SelectAll = new ActionCommand(imageListActions.SelectAll)
        {
            Text = UiStrings.SelectAll,
            Shortcut = Application.Instance.CommonModifier | Keys.A
        };
        Copy = new ActionCommand(desktopController.Copy)
        {
            Text = UiStrings.Copy,
            Shortcut = Application.Instance.CommonModifier | Keys.C
        };
        Paste = new ActionCommand(desktopController.Paste)
        {
            Text = UiStrings.Paste,
            Shortcut = Application.Instance.CommonModifier | Keys.V
        };
    }

    public DesktopCommands WithSelection(Func<ListSelection<UiImage>> selectionFunc)
    {
        return new DesktopCommands(
            _desktopController,
            _desktopScanController,
            _desktopSubFormController,
            _imageList,
            _imageListActions.WithSelection(selectionFunc),
            _thumbnailController,
            _iconProvider,
            _config);
    }

    public ImageListActions ImageListActions => _imageListActions;

    public ActionCommand Scan { get; set; }
    public ActionCommand NewProfile { get; set; }
    public ActionCommand BatchScan { get; set; }
    public ActionCommand ScannerSharing { get; set; }
    public ActionCommand Profiles { get; set; }
    public ActionCommand Ocr { get; set; }
    public ActionCommand Import { get; set; }
    public ActionCommand Squeeze { get; set; }
    public ActionCommand SqueezeAll { get; set; }
    public ActionCommand SqueezeSelected { get; set; }
    public ActionCommand SqueezeSettings { get; set; }
    public ActionCommand SaveAll { get; set; }
    public ActionCommand SaveSelected { get; set; }
    public ActionCommand SavePdf { get; set; }
    public ActionCommand SaveAllPdf { get; set; }
    public ActionCommand SaveSelectedPdf { get; set; }
    public ActionCommand PdfSettings { get; set; }
    public ActionCommand SaveImages { get; set; }
    public ActionCommand SaveAllImages { get; set; }
    public ActionCommand SaveSelectedImages { get; set; }
    public ActionCommand ImageSettings { get; set; }
    public ActionCommand EmailPdf { get; set; }
    public ActionCommand EmailAll { get; set; }
    public ActionCommand EmailSelected { get; set; }
    public ActionCommand EmailSettings { get; set; }
    public ActionCommand Print { get; set; }
    public ActionCommand ImageMenu { get; set; }
    public ActionCommand ViewImage { get; set; }
    public ActionCommand Crop { get; set; }
    public ActionCommand BrightCont { get; set; }
    public ActionCommand HueSat { get; set; }
    public ActionCommand BlackWhite { get; set; }
    public ActionCommand Sharpen { get; set; }
    public ActionCommand DocumentCorrection { get; set; }
    public ActionCommand ResetImage { get; set; }
    public ActionCommand RotateMenu { get; set; }
    public ActionCommand RotateLeft { get; set; }
    public ActionCommand RotateRight { get; set; }
    public ActionCommand Flip { get; set; }
    public ActionCommand Deskew { get; set; }
    public ActionCommand CustomRotate { get; set; }
    public ActionCommand MoveUp { get; set; }
    public ActionCommand MoveDown { get; set; }
    public ActionCommand ReorderMenu { get; set; }
    public ActionCommand Interleave { get; set; }
    public ActionCommand Deinterleave { get; set; }
    public ActionCommand AltInterleave { get; set; }
    public ActionCommand AltDeinterleave { get; set; }
    public ActionCommand ReverseMenu { get; set; }
    public ActionCommand ReverseAll { get; set; }
    public ActionCommand ReverseSelected { get; set; }
    public ActionCommand Delete { get; set; }
    public ActionCommand ClearAll { get; set; }
    public ActionCommand LanguageMenu { get; set; }
    public ActionCommand Settings { get; set; }
    public ActionCommand About { get; set; }
    public ActionCommand ZoomIn { get; set; }
    public ActionCommand ZoomOut { get; set; }
    public ActionCommand SelectAll { get; set; }
    public ActionCommand Copy { get; set; }
    public ActionCommand Paste { get; set; }
}