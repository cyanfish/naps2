using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class FViewer : FormBase
    {
        private readonly Container components = null;
        private ToolStripContainer toolStripContainer1;
        private ToolStrip toolStrip1;
        private ToolStripTextBox tbPageCurrent;
        private ToolStripLabel lblPageTotal;
        private ToolStripButton tsPrev;
        private ToolStripButton tsNext;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripDropDownButton tsdRotate;
        private ToolStripMenuItem tsRotateLeft;
        private ToolStripMenuItem tsRotateRight;
        private ToolStripMenuItem tsFlip;
        private ToolStripMenuItem tsCustomRotation;
        private ToolStripButton tsCrop;
        private ToolStripButton tsBrightnessContrast;
        private ToolStripButton tsDelete;
        private TiffViewerCtl tiffViewer1;
        private ToolStripMenuItem tsDeskew;
        private readonly ChangeTracker changeTracker;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton tsSavePDF;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton tsSaveImage;
        private readonly IOperationFactory operationFactory;
        private readonly WinFormsExportHelper exportHelper;
        private readonly AppConfigManager appConfigManager;
        private ToolStripButton tsHueSaturation;
        private ToolStripButton tsBlackWhite;
        private ToolStripButton tsSharpen;
        private readonly ScannedImageRenderer scannedImageRenderer;
        private readonly KeyboardShortcutManager ksm;
        private readonly UserConfigManager userConfigManager;

        public FViewer(ChangeTracker changeTracker, IOperationFactory operationFactory, WinFormsExportHelper exportHelper, AppConfigManager appConfigManager, ScannedImageRenderer scannedImageRenderer, KeyboardShortcutManager ksm, UserConfigManager userConfigManager)
        {
            this.changeTracker = changeTracker;
            this.operationFactory = operationFactory;
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

        protected override void OnLoad(object sender, EventArgs e)
        {
            tiffViewer1.Image = scannedImageRenderer.Render(ImageList.Images[ImageIndex]);
            tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.InvariantCulture);
            lblPageTotal.Text = string.Format(MiscResources.OfN, ImageList.Images.Count);

            if (appConfigManager.Config.HideSavePdfButton)
            {
                toolStrip1.Items.Remove(tsSavePDF);
            }
            if (appConfigManager.Config.HideSaveImagesButton)
            {
                toolStrip1.Items.Remove(tsSaveImage);
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
            tbPageCurrent.Text = (ImageIndex + 1).ToString(CultureInfo.CurrentCulture);
            SelectCallback(index);
        }

        private void UpdateImage()
        {
            tiffViewer1.Image.Dispose();
            tiffViewer1.Image = scannedImageRenderer.Render(ImageList.Images[ImageIndex]);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                tiffViewer1?.Image.Dispose();
                tiffViewer1?.Dispose();
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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.tiffViewer1 = new NAPS2.WinForms.TiffViewerCtl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbPageCurrent = new System.Windows.Forms.ToolStripTextBox();
            this.lblPageTotal = new System.Windows.Forms.ToolStripLabel();
            this.tsPrev = new System.Windows.Forms.ToolStripButton();
            this.tsNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsdRotate = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRotateRight = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDeskew = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCustomRotation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCrop = new System.Windows.Forms.ToolStripButton();
            this.tsBrightnessContrast = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsSavePDF = new System.Windows.Forms.ToolStripButton();
            this.tsSaveImage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsDelete = new System.Windows.Forms.ToolStripButton();
            this.tsSharpen = new System.Windows.Forms.ToolStripButton();
            this.tsBlackWhite = new System.Windows.Forms.ToolStripButton();
            this.tsHueSaturation = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tiffViewer1);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // tiffViewer1
            // 
            resources.ApplyResources(this.tiffViewer1, "tiffViewer1");
            this.tiffViewer1.Image = null;
            this.tiffViewer1.Name = "tiffViewer1";
            this.tiffViewer1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tiffViewer1_KeyDown);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbPageCurrent,
            this.lblPageTotal,
            this.tsPrev,
            this.tsNext,
            this.toolStripSeparator1,
            this.tsdRotate,
            this.tsCrop,
            this.tsBrightnessContrast,
            this.tsHueSaturation,
            this.tsBlackWhite,
            this.tsSharpen,
            this.toolStripSeparator3,
            this.tsSavePDF,
            this.tsSaveImage,
            this.toolStripSeparator2,
            this.tsDelete});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // tbPageCurrent
            // 
            this.tbPageCurrent.Name = "tbPageCurrent";
            resources.ApplyResources(this.tbPageCurrent, "tbPageCurrent");
            this.tbPageCurrent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbPageCurrent_KeyDown);
            this.tbPageCurrent.TextChanged += new System.EventHandler(this.tbPageCurrent_TextChanged);
            // 
            // lblPageTotal
            // 
            this.lblPageTotal.Name = "lblPageTotal";
            resources.ApplyResources(this.lblPageTotal, "lblPageTotal");
            // 
            // tsPrev
            // 
            this.tsPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsPrev.Image = global::NAPS2.Icons.arrow_left;
            resources.ApplyResources(this.tsPrev, "tsPrev");
            this.tsPrev.Name = "tsPrev";
            this.tsPrev.Click += new System.EventHandler(this.tsPrev_Click);
            // 
            // tsNext
            // 
            this.tsNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsNext.Image = global::NAPS2.Icons.arrow_right;
            resources.ApplyResources(this.tsNext, "tsNext");
            this.tsNext.Name = "tsNext";
            this.tsNext.Click += new System.EventHandler(this.tsNext_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsdRotate
            // 
            this.tsdRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsdRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsRotateLeft,
            this.tsRotateRight,
            this.tsFlip,
            this.tsDeskew,
            this.tsCustomRotation});
            this.tsdRotate.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            resources.ApplyResources(this.tsdRotate, "tsdRotate");
            this.tsdRotate.Name = "tsdRotate";
            this.tsdRotate.ShowDropDownArrow = false;
            // 
            // tsRotateLeft
            // 
            this.tsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            this.tsRotateLeft.Name = "tsRotateLeft";
            resources.ApplyResources(this.tsRotateLeft, "tsRotateLeft");
            this.tsRotateLeft.Click += new System.EventHandler(this.tsRotateLeft_Click);
            // 
            // tsRotateRight
            // 
            this.tsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise_small;
            this.tsRotateRight.Name = "tsRotateRight";
            resources.ApplyResources(this.tsRotateRight, "tsRotateRight");
            this.tsRotateRight.Click += new System.EventHandler(this.tsRotateRight_Click);
            // 
            // tsFlip
            // 
            this.tsFlip.Image = global::NAPS2.Icons.arrow_switch_small;
            this.tsFlip.Name = "tsFlip";
            resources.ApplyResources(this.tsFlip, "tsFlip");
            this.tsFlip.Click += new System.EventHandler(this.tsFlip_Click);
            // 
            // tsDeskew
            // 
            this.tsDeskew.Name = "tsDeskew";
            resources.ApplyResources(this.tsDeskew, "tsDeskew");
            this.tsDeskew.Click += new System.EventHandler(this.tsDeskew_Click);
            // 
            // tsCustomRotation
            // 
            this.tsCustomRotation.Name = "tsCustomRotation";
            resources.ApplyResources(this.tsCustomRotation, "tsCustomRotation");
            this.tsCustomRotation.Click += new System.EventHandler(this.tsCustomRotation_Click);
            // 
            // tsCrop
            // 
            this.tsCrop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsCrop.Image = global::NAPS2.Icons.transform_crop;
            resources.ApplyResources(this.tsCrop, "tsCrop");
            this.tsCrop.Name = "tsCrop";
            this.tsCrop.Click += new System.EventHandler(this.tsCrop_Click);
            // 
            // tsBrightnessContrast
            // 
            this.tsBrightnessContrast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBrightnessContrast.Image = global::NAPS2.Icons.contrast_with_sun;
            resources.ApplyResources(this.tsBrightnessContrast, "tsBrightnessContrast");
            this.tsBrightnessContrast.Name = "tsBrightnessContrast";
            this.tsBrightnessContrast.Click += new System.EventHandler(this.tsBrightnessContrast_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // tsSavePDF
            // 
            this.tsSavePDF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSavePDF.Image = global::NAPS2.Icons.file_extension_pdf_small;
            resources.ApplyResources(this.tsSavePDF, "tsSavePDF");
            this.tsSavePDF.Name = "tsSavePDF";
            this.tsSavePDF.Click += new System.EventHandler(this.tsSavePDF_Click);
            // 
            // tsSaveImage
            // 
            this.tsSaveImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSaveImage.Image = global::NAPS2.Icons.picture_small;
            resources.ApplyResources(this.tsSaveImage, "tsSaveImage");
            this.tsSaveImage.Name = "tsSaveImage";
            this.tsSaveImage.Click += new System.EventHandler(this.tsSaveImage_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // tsDelete
            // 
            this.tsDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsDelete.Image = global::NAPS2.Icons.cross_small;
            resources.ApplyResources(this.tsDelete, "tsDelete");
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // tsSharpen
            // 
            this.tsSharpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSharpen.Image = global::NAPS2.Icons.sharpen;
            resources.ApplyResources(this.tsSharpen, "tsSharpen");
            this.tsSharpen.Name = "tsSharpen";
            this.tsSharpen.Click += new System.EventHandler(this.tsSharpen_Click);
            // 
            // tsBlackWhite
            // 
            this.tsBlackWhite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBlackWhite.Image = global::NAPS2.Icons.contrast_high;
            resources.ApplyResources(this.tsBlackWhite, "tsBlackWhite");
            this.tsBlackWhite.Name = "tsBlackWhite";
            this.tsBlackWhite.Click += new System.EventHandler(this.tsBlackWhite_Click);
            // 
            // tsHueSaturation
            // 
            this.tsHueSaturation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsHueSaturation.Image = global::NAPS2.Icons.color_management;
            resources.ApplyResources(this.tsHueSaturation, "tsHueSaturation");
            this.tsHueSaturation.Name = "tsHueSaturation";
            this.tsHueSaturation.Click += new System.EventHandler(this.tsHueSaturation_Click);
            // 
            // FViewer
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "FViewer";
            this.ShowInTaskbar = false;
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void tbPageCurrent_TextChanged(object sender, EventArgs e)
        {
            int indexOffBy1;
            if (int.TryParse(tbPageCurrent.Text, out indexOffBy1))
            {
                GoTo(indexOffBy1 - 1);
            }
        }

        private void tsNext_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex + 1);
        }

        private void tsPrev_Click(object sender, EventArgs e)
        {
            GoTo(ImageIndex - 1);
        }

        private void tsRotateLeft_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate270FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsRotateRight_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate90FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsFlip_Click(object sender, EventArgs e)
        {
            ImageList.RotateFlip(Enumerable.Range(ImageIndex, 1), RotateFlipType.Rotate180FlipNone);
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsDeskew_Click(object sender, EventArgs e)
        {
            var op = operationFactory.Create<DeskewOperation>();
            var progressForm = FormFactory.Create<FProgress>();
            progressForm.Operation = op;

            if (op.Start(new[] { ImageList.Images[ImageIndex] }))
            {
                progressForm.ShowDialog();
                UpdateImage();
                UpdateCallback(Enumerable.Range(ImageIndex, 1));
            }
        }

        private void tsCustomRotation_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FRotate>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsCrop_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FCrop>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsBrightnessContrast_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBrightnessContrast>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsHueSaturation_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FHueSaturation>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsBlackWhite_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FBlackWhite>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsSharpen_Click(object sender, EventArgs e)
        {
            var form = FormFactory.Create<FSharpen>();
            form.Image = ImageList.Images[ImageIndex];
            form.ShowDialog();
            UpdateImage();
            UpdateCallback(Enumerable.Range(ImageIndex, 1));
        }

        private void tsDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDeleteItems, 1), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DeleteCurrentImage();
            }
        }

        private void DeleteCurrentImage()
        {
            // Need to dispose the bitmap first to avoid file access issues
            tiffViewer1.Image.Dispose();
            // Actually delete the image
            ImageList.Delete(Enumerable.Range(ImageIndex, 1));
            // Update FDesktop in the background
            DeleteCallback();

            if (ImageList.Images.Any())
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

        private void tsSavePDF_Click(object sender, EventArgs e)
        {
            if (exportHelper.SavePDF(new List<ScannedImage> { ImageList.Images[ImageIndex] }, null))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    DeleteCurrentImage();
                }
            }
        }

        private void tsSaveImage_Click(object sender, EventArgs e)
        {
            if (exportHelper.SaveImages(new List<ScannedImage> { ImageList.Images[ImageIndex] }, null))
            {
                if (appConfigManager.Config.DeleteAfterSaving)
                {
                    DeleteCurrentImage();
                }
            }
        }

        private void tiffViewer1_KeyDown(object sender, KeyEventArgs e)
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

        private void tbPageCurrent_KeyDown(object sender, KeyEventArgs e)
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

            ksm.Assign("Del", tsDelete);

            // Configured

            var ks = userConfigManager.Config.KeyboardShortcuts ?? appConfigManager.Config.KeyboardShortcuts ?? new KeyboardShortcuts();

            ksm.Assign(ks.Delete, tsDelete);
            ksm.Assign(ks.ImageBlackWhite, tsBlackWhite);
            ksm.Assign(ks.ImageBrightness, tsBrightnessContrast);
            ksm.Assign(ks.ImageContrast, tsBrightnessContrast);
            ksm.Assign(ks.ImageCrop, tsCrop);
            ksm.Assign(ks.ImageHue, tsHueSaturation);
            ksm.Assign(ks.ImageSaturation, tsHueSaturation);
            ksm.Assign(ks.ImageSharpen, tsSharpen);

            ksm.Assign(ks.RotateCustom, tsCustomRotation);
            ksm.Assign(ks.RotateFlip, tsFlip);
            ksm.Assign(ks.RotateLeft, tsRotateLeft);
            ksm.Assign(ks.RotateRight, tsRotateRight);
            ksm.Assign(ks.SaveImages, tsSaveImage);
            ksm.Assign(ks.SavePDF, tsSavePDF);
        }
    }
}
