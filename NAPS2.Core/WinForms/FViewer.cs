using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class FViewer : FormBase
    {
        private ToolStripContainer toolStripContainer1;
        private ToolStrip toolStrip1;
        private ToolStripTextBox TbPageCurrent;
        private ToolStripLabel lblPageTotal;
        private ToolStripButton tsPrev;
        private ToolStripButton TsNext;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripDropDownButton tsdRotate;
        private ToolStripMenuItem TsRotateLeft;
        private ToolStripMenuItem TsRotateRight;
        private ToolStripMenuItem TsFlip;
        private ToolStripMenuItem TsCustomRotation;
        private ToolStripButton TsCrop;
        private ToolStripButton TsBrightnessContrast;
        private ToolStripButton TsDelete;
        private TiffViewerCtl TiffViewer1;
        private ToolStripMenuItem TsDeskew;
        private readonly ChangeTracker changeTracker;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton TsSavePDF;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton TsSaveImage;
        private readonly IOperationFactory OperationFactory;
        private readonly WinFormsExportHelper exportHelper;
        private readonly AppConfigManager appConfigManager;
        private ToolStripButton TsHueSaturation;
        private ToolStripButton TsBlackWhite;
        private ToolStripButton TsSharpen;
        private readonly ScannedImageRenderer scannedImageRenderer;
        private readonly KeyboardShortcutManager ksm;
        private readonly UserConfigManager userConfigManager;

        public FViewer(ChangeTracker changeTracker, IOperationFactory OperationFactory, WinFormsExportHelper exportHelper, AppConfigManager appConfigManager, ScannedImageRenderer scannedImageRenderer, KeyboardShortcutManager ksm, UserConfigManager userConfigManager)
        {
            this.changeTracker = changeTracker;
            this.OperationFactory = OperationFactory;
            this.exportHelper = exportHelper;
            this.appConfigManager = appConfigManager;
            this.scannedImageRenderer = scannedImageRenderer;
            this.ksm = ksm;
            this.userConfigManager = userConfigManager;
            InitializeComponent();
        }

        public ScannedImageList ImageList { get; set; }
        public int ImageIndex { get; set; }
        public Action DeleteCallback { get; set; }
        public Action<IEnumerable<int>> UpdateCallback { get; set; }
        public Action<int> SelectCallback { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            TiffViewer1.Image = scannedImageRenderer.Render(ImageList.Images[ImageIndex]);
            TbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.InvariantCulture);
            lblPageTotal.Text = string.Format(MiscResources.OfN, ImageList.Images.Count);

            if (appConfigManager.Config.HideSavePdfButton)
            {
                toolStrip1.Items.Remove(TsSavePDF);
            }
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                toolStrip1.Items.Remove(TsSaveImage);
            }

            AssignKeyboardShortcuts();
        }

        private void GoTo(int index)
        {
            if (index == ImageIndex || index < 0 || index >= ImageList.Images.Count)
            {
                return;
            }
            ImageIndex = index;
            UpdateImage();
            TbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.CurrentCulture);
            SelectCallback(index);
        }

        private void UpdateImage()
        {
            TiffViewer1.Image.Dispose();
            TiffViewer1.Image = scannedImageRenderer.Render(ImageList.Images[ImageIndex]);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TiffViewer1?.Image.Dispose();
                TiffViewer1?.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FViewer));
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            TiffViewer1 = new NAPS2.WinForms.TiffViewerCtl();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            TbPageCurrent = new System.Windows.Forms.ToolStripTextBox();
            lblPageTotal = new System.Windows.Forms.ToolStripLabel();
            tsPrev = new System.Windows.Forms.ToolStripButton();
            TsNext = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsdRotate = new System.Windows.Forms.ToolStripDropDownButton();
            TsRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            TsRotateRight = new System.Windows.Forms.ToolStripMenuItem();
            TsFlip = new System.Windows.Forms.ToolStripMenuItem();
            TsDeskew = new System.Windows.Forms.ToolStripMenuItem();
            TsCustomRotation = new System.Windows.Forms.ToolStripMenuItem();
            TsCrop = new System.Windows.Forms.ToolStripButton();
            TsBrightnessContrast = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            TsSavePDF = new System.Windows.Forms.ToolStripButton();
            TsSaveImage = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            TsDelete = new System.Windows.Forms.ToolStripButton();
            TsSharpen = new System.Windows.Forms.ToolStripButton();
            TsBlackWhite = new System.Windows.Forms.ToolStripButton();
            TsHueSaturation = new System.Windows.Forms.ToolStripButton();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            //
            // toolStripContainer1
            //
            //
            // toolStripContainer1.ContentPanel
            //
            toolStripContainer1.ContentPanel.Controls.Add(TiffViewer1);
            resources.ApplyResources(toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(toolStripContainer1, "toolStripContainer1");
            toolStripContainer1.Name = "toolStripContainer1";
            //
            // toolStripContainer1.TopToolStripPanel
            //
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            //
            // TiffViewer1
            //
            resources.ApplyResources(TiffViewer1, "TiffViewer1");
            TiffViewer1.Image = null;
            TiffViewer1.Name = "TiffViewer1";
            TiffViewer1.KeyDown += TiffViewer1_KeyDown;
            //
            // toolStrip1
            //
            resources.ApplyResources(toolStrip1, "toolStrip1");
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            TbPageCurrent,
            lblPageTotal,
            tsPrev,
            TsNext,
            toolStripSeparator1,
            tsdRotate,
            TsCrop,
            TsBrightnessContrast,
            TsHueSaturation,
            TsBlackWhite,
            TsSharpen,
            toolStripSeparator3,
            TsSavePDF,
            TsSaveImage,
            toolStripSeparator2,
            TsDelete});
            toolStrip1.Name = "toolStrip1";
            //
            // TbPageCurrent
            //
            TbPageCurrent.Name = "TbPageCurrent";
            resources.ApplyResources(TbPageCurrent, "TbPageCurrent");
            TbPageCurrent.KeyDown += TbPageCurrent_KeyDown;
            TbPageCurrent.TextChanged += TbPageCurrent_TextChanged;
            //
            // lblPageTotal
            //
            lblPageTotal.Name = "lblPageTotal";
            resources.ApplyResources(lblPageTotal, "lblPageTotal");
            //
            // tsPrev
            //
            tsPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsPrev.Image = global::NAPS2.Icons.arrow_left;
            resources.ApplyResources(tsPrev, "tsPrev");
            tsPrev.Name = "tsPrev";
            tsPrev.Click += TsPrev_Click;
            //
            // TsNext
            //
            TsNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsNext.Image = global::NAPS2.Icons.arrow_right;
            resources.ApplyResources(TsNext, "TsNext");
            TsNext.Name = "TsNext";
            TsNext.Click += TsNext_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            //
            // tsdRotate
            //
            tsdRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsdRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            TsRotateLeft,
            TsRotateRight,
            TsFlip,
            TsDeskew,
            TsCustomRotation});
            tsdRotate.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            resources.ApplyResources(tsdRotate, "tsdRotate");
            tsdRotate.Name = "tsdRotate";
            tsdRotate.ShowDropDownArrow = false;
            //
            // TsRotateLeft
            //
            TsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            TsRotateLeft.Name = "TsRotateLeft";
            resources.ApplyResources(TsRotateLeft, "TsRotateLeft");
            TsRotateLeft.Click += TsRotateLeft_Click;
            //
            // TsRotateRight
            //
            TsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise_small;
            TsRotateRight.Name = "TsRotateRight";
            resources.ApplyResources(TsRotateRight, "TsRotateRight");
            TsRotateRight.Click += TsRotateRight_Click;
            //
            // TsFlip
            //
            TsFlip.Image = global::NAPS2.Icons.arrow_switch_small;
            TsFlip.Name = "TsFlip";
            resources.ApplyResources(TsFlip, "TsFlip");
            TsFlip.Click += TsFlip_Click;
            //
            // TsDeskew
            //
            TsDeskew.Name = "TsDeskew";
            resources.ApplyResources(TsDeskew, "TsDeskew");
            TsDeskew.Click += TsDeskew_Click;
            //
            // TsCustomRotation
            //
            TsCustomRotation.Name = "TsCustomRotation";
            resources.ApplyResources(TsCustomRotation, "TsCustomRotation");
            TsCustomRotation.Click += TsCustomRotation_Click;
            //
            // TsCrop
            //
            TsCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsCrop.Image = global::NAPS2.Icons.transform_crop;
            resources.ApplyResources(TsCrop, "TsCrop");
            TsCrop.Name = "TsCrop";
            TsCrop.Click += TsCrop_Click;
            //
            // TsBrightnessContrast
            //
            TsBrightnessContrast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsBrightnessContrast.Image = global::NAPS2.Icons.contrast_with_sun;
            resources.ApplyResources(TsBrightnessContrast, "TsBrightnessContrast");
            TsBrightnessContrast.Name = "TsBrightnessContrast";
            TsBrightnessContrast.Click += TsBrightnessContrast_Click;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            //
            // TsSavePDF
            //
            TsSavePDF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsSavePDF.Image = global::NAPS2.Icons.file_extension_pdf_small;
            resources.ApplyResources(TsSavePDF, "TsSavePDF");
            TsSavePDF.Name = "TsSavePDF";
            TsSavePDF.Click += TsSavePDF_Click;
            //
            // TsSaveImage
            //
            TsSaveImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsSaveImage.Image = global::NAPS2.Icons.picture_small;
            resources.ApplyResources(TsSaveImage, "TsSaveImage");
            TsSaveImage.Name = "TsSaveImage";
            TsSaveImage.Click += TsSaveImage_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            //
            // TsDelete
            //
            TsDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsDelete.Image = global::NAPS2.Icons.cross_small;
            resources.ApplyResources(TsDelete, "TsDelete");
            TsDelete.Name = "TsDelete";
            TsDelete.Click += TsDelete_Click;
            //
            // TsSharpen
            //
            TsSharpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsSharpen.Image = global::NAPS2.Icons.sharpen;
            resources.ApplyResources(TsSharpen, "TsSharpen");
            TsSharpen.Name = "TsSharpen";
            TsSharpen.Click += TsSharpen_Click;
            //
            // TsBlackWhite
            //
            TsBlackWhite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsBlackWhite.Image = global::NAPS2.Icons.contrast_high;
            resources.ApplyResources(TsBlackWhite, "TsBlackWhite");
            TsBlackWhite.Name = "TsBlackWhite";
            TsBlackWhite.Click += TsBlackWhite_Click;
            //
            // TsHueSaturation
            //
            TsHueSaturation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            TsHueSaturation.Image = global::NAPS2.Icons.color_management;
            resources.ApplyResources(TsHueSaturation, "TsHueSaturation");
            TsHueSaturation.Name = "TsHueSaturation";
            TsHueSaturation.Click += TsHueSaturation_Click;
            //
            // FViewer
            //
            resources.ApplyResources(this, "$this");
            Controls.Add(toolStripContainer1);
            Name = "FViewer";
            ShowInTaskbar = false;
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion Windows Form Designer generated code

        private void TbPageCurrent_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TbPageCurrent.Text, out int indexOffBy1))
            {
                GoTo(indexOffBy1 - 1);
            }
        }

        private void TsNext_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex + 1);
        }

        private void TsPrev_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex - 1);
        }

        private void TsRotateLeft_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate270FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsRotateRight_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate90FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsFlip_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate180FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsDeskew_Click(object sender, EventArgs e)
        {
            var op = OperationFactory.Create<DeskewOperation>();
            var progressForm = FormFactory.Create<FProgress>();
            progressForm.Operation = op;

            if (op.Start(new[] { ImageList.Images[ImageIndex] }))
            {
                progressForm.ShowDialog();
                UpdateImage();
                UpdateCallback(Enumerable.Range(ImageIndex, 1));
            }
        }

        private void TsCustomRotation_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FRotate>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsCrop_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FCrop>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsBrightnessContrast_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBrightnessContrast>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsHueSaturation_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FHueSaturation>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsBlackWhite_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBlackWhite>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsSharpen_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FSharpen>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void TsDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, 1), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DeleteCurrentImage();
            }
        }

        private void DeleteCurrentImage()
        {
            // Need to dispose the bitmap first to avoid file access issues
            TiffViewer1.Image.Dispose();
            // Actually delete the image
            ImageList.Delete(Enumerable.Range(ImageIndex, 1));
            // Update FDesktop in the background
            DeleteCallback();

            if (ImageList.Images.Count > 0)
            {
                changeTracker.HasUnsavedChanges = true;
                // Update the GUI for the newly displayed image
                if (ImageIndex >= ImageList.Images.Count)
                {
                    GoTo(ImageList.Images.Count - 1);
                }
                else
                {
                    UpdateImage();
                }
                lblPageTotal.Text = string.Format(MiscResources.OfN, ImageList.Images.Count);
            }
            else
            {
                changeTracker.HasUnsavedChanges = false;
                // No images left to display, so no point keeping the form open
                Close();
            }
        }

        private void TsSavePDF_Click(object sender, EventArgs e)
        {
            if (exportHelper.SavePDF(new List<ScannedImage> { ImageList.Images[ImageIndex] }, null))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    DeleteCurrentImage();
                }
            }
        }

        private void TsSaveImage_Click(object sender, EventArgs e)
        {
            if (exportHelper.SaveImages(new List<ScannedImage> { ImageList.Images[ImageIndex] }, null))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    DeleteCurrentImage();
                }
            }
        }

        private void TiffViewer1_KeyDown(object sender, KeyEventArgs e)
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
                        GoTo(ImageIndex + 1);
                        return;

                    case Keys.PageUp:
                    case Keys.Left:
                    case Keys.Up:
                        GoTo(ImageIndex - 1);
                        return;
                }
            }

            ksm.Perform(e.KeyData);
        }

        private void TbPageCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Control || e.Shift || e.Alt))
            {
                switch (e.KeyCode)
                {
                    case Keys.PageDown:
                    case Keys.Right:
                    case Keys.Down:
                        GoTo(ImageIndex + 1);
                        return;

                    case Keys.PageUp:
                    case Keys.Left:
                    case Keys.Up:
                        GoTo(ImageIndex - 1);
                        return;
                }
            }

            ksm.Perform(e.KeyData);
        }

        private void AssignKeyboardShortcuts()
        {
            // Defaults

            ksm.Assign("Del", TsDelete);

            // Configured

            var ks = userConfigManager.Config.KeyboardShortcuts ?? appConfigManager.Config.KeyboardShortcuts ?? new KeyboardShortcuts();

            ksm.Assign(ks.Delete, TsDelete);
            ksm.Assign(ks.ImageBlackWhite, TsBlackWhite);
            ksm.Assign(ks.ImageBrightness, TsBrightnessContrast);
            ksm.Assign(ks.ImageContrast, TsBrightnessContrast);
            ksm.Assign(ks.ImageCrop, TsCrop);
            ksm.Assign(ks.ImageHue, TsHueSaturation);
            ksm.Assign(ks.ImageSaturation, TsHueSaturation);
            ksm.Assign(ks.ImageSharpen, TsSharpen);

            ksm.Assign(ks.RotateCustom, TsCustomRotation);
            ksm.Assign(ks.RotateFlip, TsFlip);
            ksm.Assign(ks.RotateLeft, TsRotateLeft);
            ksm.Assign(ks.RotateRight, TsRotateRight);
            ksm.Assign(ks.SaveImages, TsSaveImage);
            ksm.Assign(ks.SavePDF, TsSavePDF);
        }
    }
}