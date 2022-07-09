using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using NAPS2.Images.Gdi;

namespace NAPS2.WinForms;

public class FViewer : FormBase
{
    private readonly Container _components = null;
    private ToolStripContainer _toolStripContainer1;
    private ToolStrip _toolStrip1;
    private ToolStripTextBox _tbPageCurrent;
    private ToolStripLabel _lblPageTotal;
    private ToolStripButton _tsPrev;
    private ToolStripButton _tsNext;
    private ToolStripSeparator _toolStripSeparator1;
    private ToolStripDropDownButton _tsdRotate;
    private ToolStripMenuItem _tsRotateLeft;
    private ToolStripMenuItem _tsRotateRight;
    private ToolStripMenuItem _tsFlip;
    private ToolStripMenuItem _tsCustomRotation;
    private ToolStripButton _tsCrop;
    private ToolStripButton _tsBrightnessContrast;
    private ToolStripButton _tsDelete;
    private TiffViewerCtl _tiffViewer1;
    private ToolStripMenuItem _tsDeskew;
    private ToolStripSeparator _toolStripSeparator3;
    private ToolStripButton _tsSavePdf;
    private ToolStripSeparator _toolStripSeparator2;
    private ToolStripButton _tsSaveImage;
    private readonly IOperationFactory _operationFactory;
    private readonly IWinFormsExportHelper _exportHelper;
    private ToolStripButton _tsHueSaturation;
    private ToolStripButton _tsBlackWhite;
    private ToolStripButton _tsSharpen;
    private readonly KeyboardShortcutManager _ksm;
    private readonly OperationProgress _operationProgress;
    private readonly GdiImageContext _imageContext;
    private readonly UiImageList _imageList;
    private readonly INotificationManager _notificationManager;

    private UiImage? _currentImage;

    public FViewer(IOperationFactory operationFactory, IWinFormsExportHelper exportHelper,
        KeyboardShortcutManager ksm, OperationProgress operationProgress, GdiImageContext imageContext,
        UiImageList imageList, INotificationManager notificationManager)
    {
        _operationFactory = operationFactory;
        _exportHelper = exportHelper;
        _ksm = ksm;
        _operationProgress = operationProgress;
        _imageContext = imageContext;
        _imageList = imageList;
        _notificationManager = notificationManager;
        InitializeComponent();
    }

    public UiImage CurrentImage
    {
        get => _currentImage ?? throw new InvalidOperationException();
        set
        {
            if (_currentImage != null)
            {
                _currentImage.ThumbnailInvalidated -= ImageThumbnailInvalidated;
            }
            _currentImage = value;
            _currentImage.ThumbnailInvalidated += ImageThumbnailInvalidated;
        }
    }

    private void ImageThumbnailInvalidated(object? sender, EventArgs e)
    {
        SafeInvokeAsync(() => UpdateImage().AssertNoAwait());
    }

    private int ImageIndex
    {
        get
        {
            var index = _imageList.Images.IndexOf(CurrentImage);
            if (index == -1)
            {
                index = 0;
            }
            return index;
        }
    }

    protected override async void OnLoad(object sender, EventArgs e)
    {
        _tbPageCurrent.Visible = PlatformCompat.Runtime.IsToolbarTextboxSupported;
        if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SavePdf))
        {
            _toolStrip1.Items.Remove(_tsSavePdf);
        }
        if (Config.Get(c => c.HiddenButtons).HasFlag(ToolbarButtons.SaveImages))
        {
            _toolStrip1.Items.Remove(_tsSaveImage);
        }

        AssignKeyboardShortcuts();
        UpdatePage();
        await UpdateImage();
    }

    private async Task GoTo(int index)
    {
        lock (_imageList)
        {
            if (index == ImageIndex || index < 0 || index >= _imageList.Images.Count)
            {
                return;
            }
            CurrentImage = _imageList.Images[index];
            _imageList.UpdateSelection(ListSelection.Of(CurrentImage));
        }
        UpdatePage();
        await UpdateImage();
    }

    private void UpdatePage()
    {
        _tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.CurrentCulture);
        _lblPageTotal.Text = string.Format(MiscResources.OfN, _imageList.Images.Count);
        if (!PlatformCompat.Runtime.IsToolbarTextboxSupported)
        {
            _lblPageTotal.Text = _tbPageCurrent.Text + ' ' + _lblPageTotal.Text;
        }
    }

    private async Task UpdateImage()
    {
        _tiffViewer1.Image?.Dispose();
        _tiffViewer1.Image = null;
        using var imageToRender = CurrentImage.GetClonedImage();
        var rendered = _imageContext.RenderToBitmap(imageToRender);
        _tiffViewer1.Image = rendered;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
            _tiffViewer1?.Image?.Dispose();
            _tiffViewer1?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(FViewer));
        this._toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
        this._tiffViewer1 = new NAPS2.WinForms.TiffViewerCtl();
        this._toolStrip1 = new System.Windows.Forms.ToolStrip();
        this._tbPageCurrent = new System.Windows.Forms.ToolStripTextBox();
        this._lblPageTotal = new System.Windows.Forms.ToolStripLabel();
        this._tsPrev = new System.Windows.Forms.ToolStripButton();
        this._tsNext = new System.Windows.Forms.ToolStripButton();
        this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this._tsdRotate = new System.Windows.Forms.ToolStripDropDownButton();
        this._tsRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
        this._tsRotateRight = new System.Windows.Forms.ToolStripMenuItem();
        this._tsFlip = new System.Windows.Forms.ToolStripMenuItem();
        this._tsDeskew = new System.Windows.Forms.ToolStripMenuItem();
        this._tsCustomRotation = new System.Windows.Forms.ToolStripMenuItem();
        this._tsCrop = new System.Windows.Forms.ToolStripButton();
        this._tsBrightnessContrast = new System.Windows.Forms.ToolStripButton();
        this._tsHueSaturation = new System.Windows.Forms.ToolStripButton();
        this._tsBlackWhite = new System.Windows.Forms.ToolStripButton();
        this._tsSharpen = new System.Windows.Forms.ToolStripButton();
        this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
        this._tsSavePdf = new System.Windows.Forms.ToolStripButton();
        this._tsSaveImage = new System.Windows.Forms.ToolStripButton();
        this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
        this._tsDelete = new System.Windows.Forms.ToolStripButton();
        this._toolStripContainer1.ContentPanel.SuspendLayout();
        this._toolStripContainer1.TopToolStripPanel.SuspendLayout();
        this._toolStripContainer1.SuspendLayout();
        this._toolStrip1.SuspendLayout();
        this.SuspendLayout();
        // 
        // toolStripContainer1
        // 
        // 
        // toolStripContainer1.ContentPanel
        // 
        this._toolStripContainer1.ContentPanel.Controls.Add(this._tiffViewer1);
        resources.ApplyResources(this._toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
        resources.ApplyResources(this._toolStripContainer1, "_toolStripContainer1");
        this._toolStripContainer1.Name = "_toolStripContainer1";
        // 
        // toolStripContainer1.TopToolStripPanel
        // 
        this._toolStripContainer1.TopToolStripPanel.Controls.Add(this._toolStrip1);
        // 
        // tiffViewer1
        // 
        resources.ApplyResources(this._tiffViewer1, "_tiffViewer1");
        this._tiffViewer1.Image = null;
        this._tiffViewer1.Name = "_tiffViewer1";
        this._tiffViewer1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tiffViewer1_KeyDown);
        // 
        // toolStrip1
        // 
        resources.ApplyResources(this._toolStrip1, "_toolStrip1");
        this._toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this._tbPageCurrent,
            this._lblPageTotal,
            this._tsPrev,
            this._tsNext,
            this._toolStripSeparator1,
            this._tsdRotate,
            this._tsCrop,
            this._tsBrightnessContrast,
            this._tsHueSaturation,
            this._tsBlackWhite,
            this._tsSharpen,
            this._toolStripSeparator3,
            this._tsSavePdf,
            this._tsSaveImage,
            this._toolStripSeparator2,
            this._tsDelete
        });
        this._toolStrip1.Name = "_toolStrip1";
        // 
        // tbPageCurrent
        // 
        this._tbPageCurrent.Name = "_tbPageCurrent";
        resources.ApplyResources(this._tbPageCurrent, "_tbPageCurrent");
        this._tbPageCurrent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPageCurrent_KeyDown);
        this._tbPageCurrent.TextChanged += new System.EventHandler(this.tbPageCurrent_TextChanged);
        // 
        // lblPageTotal
        // 
        this._lblPageTotal.Name = "_lblPageTotal";
        resources.ApplyResources(this._lblPageTotal, "_lblPageTotal");
        // 
        // tsPrev
        // 
        this._tsPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsPrev.Image = global::NAPS2.Icons.arrow_left;
        resources.ApplyResources(this._tsPrev, "_tsPrev");
        this._tsPrev.Name = "_tsPrev";
        this._tsPrev.Click += new System.EventHandler(this.tsPrev_Click);
        // 
        // tsNext
        // 
        this._tsNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsNext.Image = global::NAPS2.Icons.arrow_right;
        resources.ApplyResources(this._tsNext, "_tsNext");
        this._tsNext.Name = "_tsNext";
        this._tsNext.Click += new System.EventHandler(this.tsNext_Click);
        // 
        // toolStripSeparator1
        // 
        this._toolStripSeparator1.Name = "_toolStripSeparator1";
        resources.ApplyResources(this._toolStripSeparator1, "_toolStripSeparator1");
        // 
        // tsdRotate
        // 
        this._tsdRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsdRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this._tsRotateLeft,
            this._tsRotateRight,
            this._tsFlip,
            this._tsDeskew,
            this._tsCustomRotation
        });
        this._tsdRotate.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
        resources.ApplyResources(this._tsdRotate, "_tsdRotate");
        this._tsdRotate.Name = "_tsdRotate";
        this._tsdRotate.ShowDropDownArrow = false;
        // 
        // tsRotateLeft
        // 
        this._tsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
        this._tsRotateLeft.Name = "_tsRotateLeft";
        resources.ApplyResources(this._tsRotateLeft, "_tsRotateLeft");
        this._tsRotateLeft.Click += new System.EventHandler(this.tsRotateLeft_Click);
        // 
        // tsRotateRight
        // 
        this._tsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise_small;
        this._tsRotateRight.Name = "_tsRotateRight";
        resources.ApplyResources(this._tsRotateRight, "_tsRotateRight");
        this._tsRotateRight.Click += new System.EventHandler(this.tsRotateRight_Click);
        // 
        // tsFlip
        // 
        this._tsFlip.Image = global::NAPS2.Icons.arrow_switch_small;
        this._tsFlip.Name = "_tsFlip";
        resources.ApplyResources(this._tsFlip, "_tsFlip");
        this._tsFlip.Click += new System.EventHandler(this.tsFlip_Click);
        // 
        // tsDeskew
        // 
        this._tsDeskew.Name = "_tsDeskew";
        resources.ApplyResources(this._tsDeskew, "_tsDeskew");
        this._tsDeskew.Click += new System.EventHandler(this.tsDeskew_Click);
        // 
        // tsCustomRotation
        // 
        this._tsCustomRotation.Name = "_tsCustomRotation";
        resources.ApplyResources(this._tsCustomRotation, "_tsCustomRotation");
        this._tsCustomRotation.Click += new System.EventHandler(this.tsCustomRotation_Click);
        // 
        // tsCrop
        // 
        this._tsCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsCrop.Image = global::NAPS2.Icons.transform_crop;
        resources.ApplyResources(this._tsCrop, "_tsCrop");
        this._tsCrop.Name = "_tsCrop";
        this._tsCrop.Click += new System.EventHandler(this.tsCrop_Click);
        // 
        // tsBrightnessContrast
        // 
        this._tsBrightnessContrast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsBrightnessContrast.Image = global::NAPS2.Icons.contrast_with_sun;
        resources.ApplyResources(this._tsBrightnessContrast, "_tsBrightnessContrast");
        this._tsBrightnessContrast.Name = "_tsBrightnessContrast";
        this._tsBrightnessContrast.Click += new System.EventHandler(this.tsBrightnessContrast_Click);
        // 
        // tsHueSaturation
        // 
        this._tsHueSaturation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsHueSaturation.Image = global::NAPS2.Icons.color_management;
        resources.ApplyResources(this._tsHueSaturation, "_tsHueSaturation");
        this._tsHueSaturation.Name = "_tsHueSaturation";
        this._tsHueSaturation.Click += new System.EventHandler(this.tsHueSaturation_Click);
        // 
        // tsBlackWhite
        // 
        this._tsBlackWhite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsBlackWhite.Image = global::NAPS2.Icons.contrast_high;
        resources.ApplyResources(this._tsBlackWhite, "_tsBlackWhite");
        this._tsBlackWhite.Name = "_tsBlackWhite";
        this._tsBlackWhite.Click += new System.EventHandler(this.tsBlackWhite_Click);
        // 
        // tsSharpen
        // 
        this._tsSharpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsSharpen.Image = global::NAPS2.Icons.sharpen;
        resources.ApplyResources(this._tsSharpen, "_tsSharpen");
        this._tsSharpen.Name = "_tsSharpen";
        this._tsSharpen.Click += new System.EventHandler(this.tsSharpen_Click);
        // 
        // toolStripSeparator3
        // 
        this._toolStripSeparator3.Name = "_toolStripSeparator3";
        resources.ApplyResources(this._toolStripSeparator3, "_toolStripSeparator3");
        // 
        // tsSavePDF
        // 
        this._tsSavePdf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsSavePdf.Image = global::NAPS2.Icons.file_extension_pdf_small;
        resources.ApplyResources(this._tsSavePdf, "_tsSavePdf");
        this._tsSavePdf.Name = "_tsSavePdf";
        this._tsSavePdf.Click += new System.EventHandler(this.tsSavePDF_Click);
        // 
        // tsSaveImage
        // 
        this._tsSaveImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsSaveImage.Image = global::NAPS2.Icons.picture_small;
        resources.ApplyResources(this._tsSaveImage, "_tsSaveImage");
        this._tsSaveImage.Name = "_tsSaveImage";
        this._tsSaveImage.Click += new System.EventHandler(this.tsSaveImage_Click);
        // 
        // toolStripSeparator2
        // 
        this._toolStripSeparator2.Name = "_toolStripSeparator2";
        resources.ApplyResources(this._toolStripSeparator2, "_toolStripSeparator2");
        // 
        // tsDelete
        // 
        this._tsDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
        this._tsDelete.Image = global::NAPS2.Icons.cross_small;
        resources.ApplyResources(this._tsDelete, "_tsDelete");
        this._tsDelete.Name = "_tsDelete";
        this._tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
        // 
        // FViewer
        // 
        resources.ApplyResources(this, "$this");
        this.Controls.Add(this._toolStripContainer1);
        this.Name = "FViewer";
        this.ShowInTaskbar = false;
        this._toolStripContainer1.ContentPanel.ResumeLayout(false);
        this._toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
        this._toolStripContainer1.TopToolStripPanel.PerformLayout();
        this._toolStripContainer1.ResumeLayout(false);
        this._toolStripContainer1.PerformLayout();
        this._toolStrip1.ResumeLayout(false);
        this._toolStrip1.PerformLayout();
        this.ResumeLayout(false);
    }

    #endregion

    private async void tbPageCurrent_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(_tbPageCurrent.Text, out int indexOffBy1))
        {
            await GoTo(indexOffBy1 - 1);
        }
    }

    private async void tsNext_Click(object sender, EventArgs e)
    {
        await GoTo(ImageIndex + 1);
    }

    private async void tsPrev_Click(object sender, EventArgs e)
    {
        await GoTo(ImageIndex - 1);
    }

    private async void tsRotateLeft_Click(object sender, EventArgs e)
    {
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 270),
            ListSelection.Of(CurrentImage));
    }

    private async void tsRotateRight_Click(object sender, EventArgs e)
    {
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 90),
            ListSelection.Of(CurrentImage));
    }

    private async void tsFlip_Click(object sender, EventArgs e)
    {
        await _imageList.MutateAsync(new ImageListMutation.RotateFlip(_imageContext, 180),
            ListSelection.Of(CurrentImage));
    }

    private async void tsDeskew_Click(object sender, EventArgs e)
    {
        var op = _operationFactory.Create<DeskewOperation>();
        if (op.Start(new[] { CurrentImage }, new DeskewParams { ThumbnailSize = Config.ThumbnailSize() }))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    private async void tsCustomRotation_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FRotate>();
        form.Image = CurrentImage;
    }

    private async void tsCrop_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FCrop>();
        form.Image = CurrentImage;
        form.ShowDialog();
    }

    private async void tsBrightnessContrast_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FBrightnessContrast>();
        form.Image = CurrentImage;
        form.ShowDialog();
    }

    private async void tsHueSaturation_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FHueSaturation>();
        form.Image = CurrentImage;
        form.ShowDialog();
    }

    private async void tsBlackWhite_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FBlackWhite>();
        form.Image = CurrentImage;
        form.ShowDialog();
    }

    private async void tsSharpen_Click(object sender, EventArgs e)
    {
        var form = FormFactory.Create<FSharpen>();
        form.Image = CurrentImage;
        form.ShowDialog();
    }

    private async void tsDelete_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, 1), MiscResources.Delete,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
        {
            await DeleteCurrentImage();
        }
    }

    private async Task DeleteCurrentImage()
    {
        // TODO: Are the file access issues still a thing?
        // Need to dispose the bitmap first to avoid file access issues
        _tiffViewer1.Image?.Dispose();

        var lastIndex = ImageIndex;
        await _imageList.MutateAsync(new ImageListMutation.DeleteSelected(),
            ListSelection.Of(CurrentImage));

        bool shouldClose = false;
        lock (_imageList)
        {
            if (_imageList.Images.Any())
            {
                // Update the GUI for the newly displayed image
                var nextIndex = lastIndex >= _imageList.Images.Count ? _imageList.Images.Count - 1 : lastIndex;
                CurrentImage = _imageList.Images[nextIndex];
            }
            else
            {
                shouldClose = true;
            }
        }
        if (shouldClose)
        {
            // No images left to display, so no point keeping the form open
            Close();
        }
        else
        {
            UpdatePage();
            await UpdateImage();
        }
    }

    private async void tsSavePDF_Click(object sender, EventArgs e)
    {
        using var imageToSave = CurrentImage.GetClonedImage();
        if (await _exportHelper.SavePDF(new List<ProcessedImage> { imageToSave }, _notificationManager))
        {
            if (Config.Get(c => c.DeleteAfterSaving))
            {
                await DeleteCurrentImage();
            }
        }
    }

    private async void tsSaveImage_Click(object sender, EventArgs e)
    {
        using var imageToSave = CurrentImage.GetClonedImage();
        if (await _exportHelper.SaveImages(new List<ProcessedImage> { imageToSave }, _notificationManager))
        {
            if (Config.Get(c => c.DeleteAfterSaving))
            {
                await DeleteCurrentImage();
            }
        }
    }

    private async void tiffViewer1_KeyDown(object sender, KeyEventArgs e)
    {
        if (!(e.Control || e.Shift || e.Alt))
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    return;
                case Keys.PageDown:
                case Keys.Right:
                case Keys.Down:
                    await GoTo(ImageIndex + 1);
                    return;
                case Keys.PageUp:
                case Keys.Left:
                case Keys.Up:
                    await GoTo(ImageIndex - 1);
                    return;
            }
        }

        e.Handled = _ksm.Perform(e.KeyData);
    }

    private async void tbPageCurrent_KeyDown(object sender, KeyEventArgs e)
    {
        if (!(e.Control || e.Shift || e.Alt))
        {
            switch (e.KeyCode)
            {
                case Keys.PageDown:
                case Keys.Right:
                case Keys.Down:
                    await GoTo(ImageIndex + 1);
                    return;
                case Keys.PageUp:
                case Keys.Left:
                case Keys.Up:
                    await GoTo(ImageIndex - 1);
                    return;
            }
        }

        e.Handled = _ksm.Perform(e.KeyData);
    }

    private void AssignKeyboardShortcuts()
    {
        // Defaults

        _ksm.Assign("Del", _tsDelete);

        // Configured

        // TODO: Granular
        var ks = Config.Get(c => c.KeyboardShortcuts);

        _ksm.Assign(ks.Delete, _tsDelete);
        _ksm.Assign(ks.ImageBlackWhite, _tsBlackWhite);
        _ksm.Assign(ks.ImageBrightness, _tsBrightnessContrast);
        _ksm.Assign(ks.ImageContrast, _tsBrightnessContrast);
        _ksm.Assign(ks.ImageCrop, _tsCrop);
        _ksm.Assign(ks.ImageHue, _tsHueSaturation);
        _ksm.Assign(ks.ImageSaturation, _tsHueSaturation);
        _ksm.Assign(ks.ImageSharpen, _tsSharpen);

        _ksm.Assign(ks.RotateCustom, _tsCustomRotation);
        _ksm.Assign(ks.RotateFlip, _tsFlip);
        _ksm.Assign(ks.RotateLeft, _tsRotateLeft);
        _ksm.Assign(ks.RotateRight, _tsRotateRight);
        _ksm.Assign(ks.SaveImages, _tsSaveImage);
        _ksm.Assign(ks.SavePDF, _tsSavePdf);
    }
}