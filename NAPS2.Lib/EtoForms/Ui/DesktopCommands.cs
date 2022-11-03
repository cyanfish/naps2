using Eto.Forms;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.EtoForms.Ui;

public class DesktopCommands
{
    public DesktopCommands(DesktopController desktopController, DesktopScanController desktopScanController,
        IDesktopSubFormController desktopSubFormController, UiImageList imageList, ImageListActions imageListActions,
        ThumbnailController thumbnailController)
    {
        Scan = new ActionCommand(desktopScanController.ScanDefault)
        {
            ToolBarText = UiStrings.Scan,
            MenuText = UiStrings.Scan,
            Image = Icons.control_play_blue.ToEtoImage(),
            MacSymbol = "play"
        };
        NewProfile = new ActionCommand(desktopScanController.ScanWithNewProfile)
        {
            MenuText = UiStrings.NewProfile,
            Image = Icons.add_small.ToEtoImage()
        };
        BatchScan = new ActionCommand(desktopSubFormController.ShowBatchScanForm)
        {
            MenuText = UiStrings.BatchScan,
            Image = Icons.application_cascade.ToEtoImage()
        };
        Profiles = new ActionCommand(desktopSubFormController.ShowProfilesForm)
        {
            ToolBarText = UiStrings.Profiles,
            Image = Icons.blueprints.ToEtoImage(),
            MacSymbol = "list.bullet"
        };
        Ocr = new ActionCommand(desktopSubFormController.ShowOcrForm)
        {
            ToolBarText = UiStrings.Ocr,
            MenuText = UiStrings.Ocr,
            Image = Icons.text.ToEtoImage()
        };
        Import = new ActionCommand(desktopController.Import)
        {
            ToolBarText = UiStrings.Import,
            MenuText = UiStrings.Import,
            Image = Icons.folder_picture.ToEtoImage(),
            Shortcut = Application.Instance.CommonModifier | Keys.O,
            MacSymbol = "folder"
        };
        Save = new ActionCommand
        {
            ToolBarText = UiStrings.Save,
            MacSymbol = "square.and.arrow.down"
        };
        SavePdf = new ActionCommand(desktopController.SavePdf)
        {
            ToolBarText = UiStrings.SavePdf,
            Image = Icons.file_extension_pdf.ToEtoImage()
        };
        SaveAllPdf = new ActionCommand(() => desktopController.SavePDF(imageList.Images))
        {
            MenuText = UiStrings.SaveAllAsPdf
        };
        SaveSelectedPdf = new ActionCommand(() => desktopController.SavePDF(imageList.Selection))
        {
            MenuText = UiStrings.SaveSelectedAsPdf
        };
        PdfSettings = new ActionCommand(desktopSubFormController.ShowPdfSettingsForm)
        {
            MenuText = UiStrings.PdfSettings
        };
        SaveImages = new ActionCommand(desktopController.SaveImages)
        {
            ToolBarText = UiStrings.SaveImages,
            Image = Icons.pictures.ToEtoImage()
        };
        SaveAllImages = new ActionCommand(() => desktopController.SaveImages(imageList.Images))
        {
            MenuText = UiStrings.SaveAllAsImages
        };
        SaveSelectedImages = new ActionCommand(() => desktopController.SaveImages(imageList.Selection))
        {
            MenuText = UiStrings.SaveSelectedAsImages
        };
        ImageSettings = new ActionCommand(desktopSubFormController.ShowImageSettingsForm)
        {
            MenuText = UiStrings.ImageSettings
        };
        EmailPdf = new ActionCommand(desktopController.EmailPdf)
        {
            ToolBarText = UiStrings.EmailPdf,
            Image = Icons.email_attach.ToEtoImage()
        };
        EmailAllPdf = new ActionCommand(() => desktopController.EmailPDF(imageList.Images))
        {
            MenuText = UiStrings.EmailAllAsPdf
        };
        EmailSelectedPdf = new ActionCommand(() => desktopController.EmailPDF(imageList.Selection))
        {
            MenuText = UiStrings.EmailSelectedAsPdf
        };
        EmailSettings = new ActionCommand(desktopSubFormController.ShowEmailSettingsForm)
        {
            MenuText = UiStrings.EmailSettings
        };
        Print = new ActionCommand(desktopController.Print)
        {
            ToolBarText = UiStrings.Print,
            MenuText = UiStrings.Print,
            Image = Icons.printer.ToEtoImage(),
            Shortcut = Application.Instance.CommonModifier | Keys.P
        };
        ImageMenu = new Command
        {
            ToolBarText = UiStrings.Image,
            MenuText = UiStrings.Image,
            Image = Icons.picture_edit.ToEtoImage()
        };
        ViewImage = new ActionCommand(desktopSubFormController.ShowViewerForm)
        {
            ToolBarText = UiStrings.View,
            MenuText = UiStrings.View,
            MacSymbol = "viewfinder"
        };
        Crop = new ActionCommand(desktopSubFormController.ShowCropForm)
        {
            MenuText = UiStrings.Crop,
            Image = Icons.transform_crop.ToEtoImage()
        };
        BrightCont = new ActionCommand(desktopSubFormController.ShowBrightnessContrastForm)
        {
            MenuText = UiStrings.BrightnessContrast,
            Image = Icons.contrast_with_sun.ToEtoImage()
        };
        HueSat = new ActionCommand(desktopSubFormController.ShowHueSaturationForm)
        {
            MenuText = UiStrings.HueSaturation,
            Image = Icons.color_management.ToEtoImage()
        };
        BlackWhite = new ActionCommand(desktopSubFormController.ShowBlackWhiteForm)
        {
            MenuText = UiStrings.BlackAndWhite,
            Image = Icons.contrast_high.ToEtoImage()
        };
        Sharpen = new ActionCommand(desktopSubFormController.ShowSharpenForm)
        {
            MenuText = UiStrings.Sharpen,
            Image = Icons.sharpen.ToEtoImage()
        };
        // TODO: Make this an image form with options
        DocumentCorrection = new ActionCommand(desktopController.RunDocumentCorrection)
        {
            MenuText = UiStrings.DocumentCorrection
        };
        ResetImage = new ActionCommand(desktopController.ResetImage)
        {
            MenuText = UiStrings.Reset
        };
        RotateMenu = new ActionCommand
        {
            ToolBarText = UiStrings.Rotate,
            Image = Icons.arrow_rotate_anticlockwise.ToEtoImage(),
            MacSymbol = "arrow.counterclockwise"
        };
        RotateLeft = new ActionCommand(imageListActions.RotateLeft)
        {
            MenuText = UiStrings.RotateLeft,
            Image = Icons.arrow_rotate_anticlockwise_small.ToEtoImage(),
            MacSymbol = "arrow.counterclockwise"
        };
        RotateRight = new ActionCommand(imageListActions.RotateRight)
        {
            MenuText = UiStrings.RotateRight,
            Image = Icons.arrow_rotate_clockwise_small.ToEtoImage(),
            MacSymbol = "arrow.clockwise"
        };
        Flip = new ActionCommand(imageListActions.Flip)
        {
            MenuText = UiStrings.Flip,
            Image = Icons.arrow_switch_small.ToEtoImage(),
            MacSymbol = "arrow.2.squarepath"
        };
        Deskew = new ActionCommand(imageListActions.Deskew)
        {
            MenuText = UiStrings.Deskew
        };
        CustomRotate = new ActionCommand(desktopSubFormController.ShowRotateForm)
        {
            MenuText = UiStrings.CustomRotation
        };
        MoveUp = new ActionCommand(imageListActions.MoveUp)
        {
            ToolBarText = UiStrings.MoveUp,
            MenuText = UiStrings.MoveUp,
            Image = Icons.arrow_up_small.ToEtoImage(),
            MacSymbol = "arrow.up"
        };
        MoveDown = new ActionCommand(imageListActions.MoveDown)
        {
            ToolBarText = UiStrings.MoveDown,
            MenuText = UiStrings.MoveDown,
            Image = Icons.arrow_down_small.ToEtoImage(),
            MacSymbol = "arrow.down"
        };
        ReorderMenu = new Command
        {
            ToolBarText = UiStrings.Reorder,
            Image = Icons.arrow_refresh.ToEtoImage()
        };
        Interleave = new ActionCommand(imageListActions.Interleave)
        {
            MenuText = UiStrings.Interleave
        };
        Deinterleave = new ActionCommand(imageListActions.Deinterleave)
        {
            MenuText = UiStrings.Deinterleave
        };
        AltInterleave = new ActionCommand(imageListActions.AltInterleave)
        {
            MenuText = UiStrings.AltInterleave
        };
        AltDeinterleave = new ActionCommand(imageListActions.AltDeinterleave)
        {
            MenuText = UiStrings.AltDeinterleave
        };
        ReverseMenu = new Command
        {
            MenuText = UiStrings.Reverse
        };
        ReverseAll = new ActionCommand(imageListActions.ReverseAll);
        ReverseSelected = new ActionCommand(imageListActions.ReverseSelected);
        Delete = new ActionCommand(desktopController.Delete)
        {
            ToolBarText = UiStrings.Delete,
            MenuText = UiStrings.Delete,
            Image = Icons.cross.ToEtoImage()
        };
        ClearAll = new ActionCommand(desktopController.Clear)
        {
            ToolBarText = UiStrings.Clear,
            MenuText = UiStrings.ClearAll,
            Image = Icons.cancel.ToEtoImage(),
            Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.Delete
        };
        LanguageMenu = new Command
        {
            ToolBarText = UiStrings.Language,
            MenuText = UiStrings.Language,
            Image = Icons.world.ToEtoImage()
        };
        About = new ActionCommand(desktopSubFormController.ShowAboutForm)
        {
            ToolBarText = UiStrings.About,
            MenuText = UiStrings.About,
            Image = Icons.information.ToEtoImage()
        };
        ZoomIn = new ActionCommand(() => thumbnailController.StepSize(1))
        {
            ToolBarText = UiStrings.ZoomIn,
            MenuText = UiStrings.ZoomIn,
            Image = Icons.zoom_in.ToEtoImage()
        };
        ZoomOut = new ActionCommand(() => thumbnailController.StepSize(-1))
        {
            ToolBarText = UiStrings.ZoomOut,
            MenuText = UiStrings.ZoomOut,
            Image = Icons.zoom_out.ToEtoImage()
        };
    }

    public ActionCommand Scan { get; set; }
    public ActionCommand NewProfile { get; set; }
    public ActionCommand BatchScan { get; set; }
    public ActionCommand Profiles { get; set; }
    public ActionCommand Ocr { get; set; }
    public ActionCommand Import { get; set; }
    public ActionCommand Save { get; set; }
    public ActionCommand SavePdf { get; set; }
    public ActionCommand SaveAllPdf { get; set; }
    public ActionCommand SaveSelectedPdf { get; set; }
    public ActionCommand PdfSettings { get; set; }
    public ActionCommand SaveImages { get; set; }
    public ActionCommand SaveAllImages { get; set; }
    public ActionCommand SaveSelectedImages { get; set; }
    public ActionCommand ImageSettings { get; set; }
    public ActionCommand EmailPdf { get; set; }
    public ActionCommand EmailAllPdf { get; set; }
    public ActionCommand EmailSelectedPdf { get; set; }
    public ActionCommand EmailSettings { get; set; }
    public ActionCommand Print { get; set; }
    public Command ImageMenu { get; set; }
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
    public Command ReorderMenu { get; set; }
    public ActionCommand Interleave { get; set; }
    public ActionCommand Deinterleave { get; set; }
    public ActionCommand AltInterleave { get; set; }
    public ActionCommand AltDeinterleave { get; set; }
    public Command ReverseMenu { get; set; }
    public ActionCommand ReverseAll { get; set; }
    public ActionCommand ReverseSelected { get; set; }
    public ActionCommand Delete { get; set; }
    public ActionCommand ClearAll { get; set; }
    public Command LanguageMenu { get; set; }
    public ActionCommand About { get; set; }
    public ActionCommand ZoomIn { get; set; }
    public ActionCommand ZoomOut { get; set; }
}