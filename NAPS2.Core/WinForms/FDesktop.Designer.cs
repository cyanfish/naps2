using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FDesktop : IDisposable
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FDesktop));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.BtnZoomIn = new System.Windows.Forms.Button();
            this.BtnZoomOut = new System.Windows.Forms.Button();
            this.BtnZoomMouseCatcher = new System.Windows.Forms.Button();
            this.ThumbnailList1 = new NAPS2.WinForms.ThumbnailList();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxView = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.TStrip = new System.Windows.Forms.ToolStrip();
            this.TsScan = new System.Windows.Forms.ToolStripSplitButton();
            this.TsNewProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsBatchScan = new System.Windows.Forms.ToolStripMenuItem();
            this.tsProfiles = new System.Windows.Forms.ToolStripButton();
            this.tsOcr = new System.Windows.Forms.ToolStripButton();
            this.tsImport = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsdSavePDF = new System.Windows.Forms.ToolStripSplitButton();
            this.TsSavePDFAll = new System.Windows.Forms.ToolStripMenuItem();
            this.TsSavePDFSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.tsPDFSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsdSaveImages = new System.Windows.Forms.ToolStripSplitButton();
            this.TsSaveImagesAll = new System.Windows.Forms.ToolStripMenuItem();
            this.TsSaveImagesSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.tsImageSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsdEmailPDF = new System.Windows.Forms.ToolStripSplitButton();
            this.tsEmailPDFAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEmailPDFSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.tsEmailSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsPdfSettings2 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsPrint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsdImage = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.TsCrop = new System.Windows.Forms.ToolStripMenuItem();
            this.TsBrightnessContrast = new System.Windows.Forms.ToolStripMenuItem();
            this.TsSharpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tsReset = new System.Windows.Forms.ToolStripMenuItem();
            this.tsdRotate = new System.Windows.Forms.ToolStripDropDownButton();
            this.TsRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.TsRotateRight = new System.Windows.Forms.ToolStripMenuItem();
            this.TsFlip = new System.Windows.Forms.ToolStripMenuItem();
            this.TsDeskew = new System.Windows.Forms.ToolStripMenuItem();
            this.TsCustomRotation = new System.Windows.Forms.ToolStripMenuItem();
            this.tsMove = new NAPS2.WinForms.ToolStripDoubleButton();
            this.tsdReorder = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsInterleave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDeinterleave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.tsAltInterleave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAltDeinterleave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsReverse = new System.Windows.Forms.ToolStripMenuItem();
            this.tsReverseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsReverseSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.TsDelete = new System.Windows.Forms.ToolStripButton();
            this.tsClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsAbout = new System.Windows.Forms.ToolStripButton();
            this.TsHueSaturation = new System.Windows.Forms.ToolStripMenuItem();
            this.TsBlackWhite = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.TStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.BtnZoomIn);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.BtnZoomOut);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.BtnZoomMouseCatcher);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.ThumbnailList1);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.TStrip);
            // 
            // BtnZoomIn
            // 
            this.BtnZoomIn.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.BtnZoomIn, "BtnZoomIn");
            this.BtnZoomIn.Image = global::NAPS2.Icons.zoom_in;
            this.BtnZoomIn.Name = "BtnZoomIn";
            this.BtnZoomIn.UseVisualStyleBackColor = false;
            this.BtnZoomIn.Click += new System.EventHandler(this.BtnZoomIn_Click);
            // 
            // BtnZoomOut
            // 
            this.BtnZoomOut.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.BtnZoomOut, "BtnZoomOut");
            this.BtnZoomOut.Image = global::NAPS2.Icons.zoom_out;
            this.BtnZoomOut.Name = "BtnZoomOut";
            this.BtnZoomOut.UseVisualStyleBackColor = false;
            this.BtnZoomOut.Click += new System.EventHandler(this.BtnZoomOut_Click);
            // 
            // BtnZoomMouseCatcher
            // 
            this.BtnZoomMouseCatcher.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.BtnZoomMouseCatcher, "BtnZoomMouseCatcher");
            this.BtnZoomMouseCatcher.Name = "BtnZoomMouseCatcher";
            this.BtnZoomMouseCatcher.UseVisualStyleBackColor = false;
            // 
            // ThumbnailList1
            // 
            this.ThumbnailList1.AllowDrop = true;
            this.ThumbnailList1.ContextMenuStrip = this.contextMenuStrip;
            resources.ApplyResources(this.ThumbnailList1, "ThumbnailList1");
            this.ThumbnailList1.Name = "ThumbnailList1";
            this.ThumbnailList1.ThumbnailSize = new System.Drawing.Size(128, 128);
            this.ThumbnailList1.UseCompatibleStateImageBehavior = false;
            this.ThumbnailList1.ItemActivate += new System.EventHandler(this.ThumbnailList1_ItemActivate);
            this.ThumbnailList1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ThumbnailList1_ItemDrag);
            this.ThumbnailList1.SelectedIndexChanged += new System.EventHandler(this.ThumbnailList1_SelectedIndexChanged);
            this.ThumbnailList1.DragDrop += new System.Windows.Forms.DragEventHandler(this.ThumbnailList1_DragDrop);
            this.ThumbnailList1.DragEnter += new System.Windows.Forms.DragEventHandler(this.ThumbnailList1_DragEnter);
            this.ThumbnailList1.DragOver += new System.Windows.Forms.DragEventHandler(this.ThumbnailList1_DragOver);
            this.ThumbnailList1.DragLeave += new System.EventHandler(this.ThumbnailList1_DragLeave);
            this.ThumbnailList1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ThumbnailList1_KeyDown);
            this.ThumbnailList1.MouseLeave += new System.EventHandler(this.ThumbnailList1_MouseLeave);
            this.ThumbnailList1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ThumbnailList1_MouseMove);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxView,
            this.ctxSeparator1,
            this.ctxSelectAll,
            this.ctxCopy,
            this.ctxPaste,
            this.ctxSeparator2,
            this.ctxDelete});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // ctxView
            // 
            resources.ApplyResources(this.ctxView, "ctxView");
            this.ctxView.Name = "ctxView";
            this.ctxView.Click += new System.EventHandler(this.ctxView_Click);
            // 
            // ctxSeparator1
            // 
            this.ctxSeparator1.Name = "ctxSeparator1";
            resources.ApplyResources(this.ctxSeparator1, "ctxSeparator1");
            // 
            // ctxSelectAll
            // 
            this.ctxSelectAll.Name = "ctxSelectAll";
            resources.ApplyResources(this.ctxSelectAll, "ctxSelectAll");
            this.ctxSelectAll.Click += new System.EventHandler(this.ctxSelectAll_Click);
            // 
            // ctxCopy
            // 
            this.ctxCopy.Name = "ctxCopy";
            resources.ApplyResources(this.ctxCopy, "ctxCopy");
            this.ctxCopy.Click += new System.EventHandler(this.ctxCopy_Click);
            // 
            // ctxPaste
            // 
            this.ctxPaste.Name = "ctxPaste";
            resources.ApplyResources(this.ctxPaste, "ctxPaste");
            this.ctxPaste.Click += new System.EventHandler(this.ctxPaste_Click);
            // 
            // ctxSeparator2
            // 
            this.ctxSeparator2.Name = "ctxSeparator2";
            resources.ApplyResources(this.ctxSeparator2, "ctxSeparator2");
            // 
            // ctxDelete
            // 
            this.ctxDelete.Name = "ctxDelete";
            resources.ApplyResources(this.ctxDelete, "ctxDelete");
            this.ctxDelete.Click += new System.EventHandler(this.ctxDelete_Click);
            // 
            // TStrip
            // 
            resources.ApplyResources(this.TStrip, "TStrip");
            this.TStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.TStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TsScan,
            this.tsProfiles,
            this.tsOcr,
            this.tsImport,
            this.toolStripSeparator5,
            this.tsdSavePDF,
            this.tsdSaveImages,
            this.tsdEmailPDF,
            this.tsPrint,
            this.toolStripSeparator4,
            this.tsdImage,
            this.tsdRotate,
            this.tsMove,
            this.tsdReorder,
            this.toolStripSeparator2,
            this.TsDelete,
            this.tsClear,
            this.toolStripSeparator3,
            this.toolStripDropDownButton1,
            this.tsAbout});
            this.TStrip.Name = "TStrip";
            this.TStrip.ShowItemToolTips = false;
            this.TStrip.DockChanged += new System.EventHandler(this.TStrip_DockChanged);
            // 
            // TsScan
            // 
            this.TsScan.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TsNewProfile,
            this.tsBatchScan});
            this.TsScan.Image = global::NAPS2.Icons.control_play_blue;
            resources.ApplyResources(this.TsScan, "TsScan");
            this.TsScan.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.TsScan.Name = "TsScan";
            this.TsScan.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.TsScan.ButtonClick += new System.EventHandler(this.TsScan_ButtonClick);
            // 
            // TsNewProfile
            // 
            this.TsNewProfile.Image = global::NAPS2.Icons.add_small;
            resources.ApplyResources(this.TsNewProfile, "TsNewProfile");
            this.TsNewProfile.Name = "TsNewProfile";
            this.TsNewProfile.Click += new System.EventHandler(this.TsNewProfile_Click);
            // 
            // tsBatchScan
            // 
            this.tsBatchScan.Image = global::NAPS2.Icons.application_cascade;
            resources.ApplyResources(this.tsBatchScan, "tsBatchScan");
            this.tsBatchScan.Name = "tsBatchScan";
            this.tsBatchScan.Click += new System.EventHandler(this.tsBatchScan_Click);
            // 
            // tsProfiles
            // 
            resources.ApplyResources(this.tsProfiles, "tsProfiles");
            this.tsProfiles.Name = "tsProfiles";
            this.tsProfiles.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsProfiles.Click += new System.EventHandler(this.tsProfiles_Click);
            // 
            // tsOcr
            // 
            this.tsOcr.Image = global::NAPS2.Icons.text;
            resources.ApplyResources(this.tsOcr, "tsOcr");
            this.tsOcr.Name = "tsOcr";
            this.tsOcr.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsOcr.Click += new System.EventHandler(this.tsOcr_Click);
            // 
            // tsImport
            // 
            this.tsImport.Image = global::NAPS2.Icons.folder_picture;
            resources.ApplyResources(this.tsImport, "tsImport");
            this.tsImport.Name = "tsImport";
            this.tsImport.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsImport.Click += new System.EventHandler(this.tsImport_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // tsdSavePDF
            // 
            this.tsdSavePDF.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TsSavePDFAll,
            this.TsSavePDFSelected,
            this.toolStripSeparator10,
            this.tsPDFSettings});
            this.tsdSavePDF.Image = global::NAPS2.Icons.file_extension_pdf;
            resources.ApplyResources(this.tsdSavePDF, "tsdSavePDF");
            this.tsdSavePDF.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.tsdSavePDF.Name = "tsdSavePDF";
            this.tsdSavePDF.ButtonClick += new System.EventHandler(this.tsdSavePDF_ButtonClick);
            // 
            // TsSavePDFAll
            // 
            this.TsSavePDFAll.Name = "TsSavePDFAll";
            resources.ApplyResources(this.TsSavePDFAll, "TsSavePDFAll");
            this.TsSavePDFAll.Click += new System.EventHandler(this.TsSavePDFAll_Click);
            // 
            // TsSavePDFSelected
            // 
            this.TsSavePDFSelected.Name = "TsSavePDFSelected";
            resources.ApplyResources(this.TsSavePDFSelected, "TsSavePDFSelected");
            this.TsSavePDFSelected.Click += new System.EventHandler(this.TsSavePDFSelected_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // tsPDFSettings
            // 
            this.tsPDFSettings.Name = "tsPDFSettings";
            resources.ApplyResources(this.tsPDFSettings, "tsPDFSettings");
            this.tsPDFSettings.Click += new System.EventHandler(this.tsPDFSettings_Click);
            // 
            // tsdSaveImages
            // 
            this.tsdSaveImages.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TsSaveImagesAll,
            this.TsSaveImagesSelected,
            this.toolStripSeparator11,
            this.tsImageSettings});
            this.tsdSaveImages.Image = global::NAPS2.Icons.pictures;
            resources.ApplyResources(this.tsdSaveImages, "tsdSaveImages");
            this.tsdSaveImages.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.tsdSaveImages.Name = "tsdSaveImages";
            this.tsdSaveImages.ButtonClick += new System.EventHandler(this.tsdSaveImages_ButtonClick);
            // 
            // TsSaveImagesAll
            // 
            this.TsSaveImagesAll.Name = "TsSaveImagesAll";
            resources.ApplyResources(this.TsSaveImagesAll, "TsSaveImagesAll");
            this.TsSaveImagesAll.Click += new System.EventHandler(this.TsSaveImagesAll_Click);
            // 
            // TsSaveImagesSelected
            // 
            this.TsSaveImagesSelected.Name = "TsSaveImagesSelected";
            resources.ApplyResources(this.TsSaveImagesSelected, "TsSaveImagesSelected");
            this.TsSaveImagesSelected.Click += new System.EventHandler(this.TsSaveImagesSelected_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            resources.ApplyResources(this.toolStripSeparator11, "toolStripSeparator11");
            // 
            // tsImageSettings
            // 
            this.tsImageSettings.Name = "tsImageSettings";
            resources.ApplyResources(this.tsImageSettings, "tsImageSettings");
            this.tsImageSettings.Click += new System.EventHandler(this.tsImageSettings_Click);
            // 
            // tsdEmailPDF
            // 
            this.tsdEmailPDF.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsEmailPDFAll,
            this.tsEmailPDFSelected,
            this.toolStripSeparator9,
            this.tsEmailSettings,
            this.tsPdfSettings2});
            this.tsdEmailPDF.Image = global::NAPS2.Icons.email_attach;
            resources.ApplyResources(this.tsdEmailPDF, "tsdEmailPDF");
            this.tsdEmailPDF.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.tsdEmailPDF.Name = "tsdEmailPDF";
            this.tsdEmailPDF.ButtonClick += new System.EventHandler(this.tsdEmailPDF_ButtonClick);
            // 
            // tsEmailPDFAll
            // 
            this.tsEmailPDFAll.Name = "tsEmailPDFAll";
            resources.ApplyResources(this.tsEmailPDFAll, "tsEmailPDFAll");
            this.tsEmailPDFAll.Click += new System.EventHandler(this.tsEmailPDFAll_Click);
            // 
            // tsEmailPDFSelected
            // 
            this.tsEmailPDFSelected.Name = "tsEmailPDFSelected";
            resources.ApplyResources(this.tsEmailPDFSelected, "tsEmailPDFSelected");
            this.tsEmailPDFSelected.Click += new System.EventHandler(this.tsEmailPDFSelected_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
            // 
            // tsEmailSettings
            // 
            this.tsEmailSettings.Name = "tsEmailSettings";
            resources.ApplyResources(this.tsEmailSettings, "tsEmailSettings");
            this.tsEmailSettings.Click += new System.EventHandler(this.tsEmailSettings_Click);
            // 
            // tsPdfSettings2
            // 
            this.tsPdfSettings2.Name = "tsPdfSettings2";
            resources.ApplyResources(this.tsPdfSettings2, "tsPdfSettings2");
            this.tsPdfSettings2.Click += new System.EventHandler(this.tsPdfSettings2_Click);
            // 
            // tsPrint
            // 
            this.tsPrint.Image = global::NAPS2.Icons.printer;
            resources.ApplyResources(this.tsPrint, "tsPrint");
            this.tsPrint.Name = "tsPrint";
            this.tsPrint.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsPrint.Click += new System.EventHandler(this.tsPrint_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // tsdImage
            // 
            this.tsdImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsView,
            this.toolStripSeparator6,
            this.TsCrop,
            this.TsBrightnessContrast,
            this.TsHueSaturation,
            this.TsBlackWhite,
            this.TsSharpen,
            this.toolStripSeparator7,
            this.tsReset});
            this.tsdImage.Image = global::NAPS2.Icons.picture_edit;
            resources.ApplyResources(this.tsdImage, "tsdImage");
            this.tsdImage.Name = "tsdImage";
            this.tsdImage.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsdImage.ShowDropDownArrow = false;
            // 
            // tsView
            // 
            this.tsView.Name = "tsView";
            resources.ApplyResources(this.tsView, "tsView");
            this.tsView.Click += new System.EventHandler(this.tsView_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // TsCrop
            // 
            this.TsCrop.Image = global::NAPS2.Icons.transform_crop;
            resources.ApplyResources(this.TsCrop, "TsCrop");
            this.TsCrop.Name = "TsCrop";
            this.TsCrop.Click += new System.EventHandler(this.TsCrop_Click);
            // 
            // TsBrightnessContrast
            // 
            this.TsBrightnessContrast.Image = global::NAPS2.Icons.contrast_with_sun;
            resources.ApplyResources(this.TsBrightnessContrast, "TsBrightnessContrast");
            this.TsBrightnessContrast.Name = "TsBrightnessContrast";
            this.TsBrightnessContrast.Click += new System.EventHandler(this.TsBrightnessContrast_Click);
            // 
            // TsSharpen
            // 
            this.TsSharpen.Image = global::NAPS2.Icons.sharpen;
            resources.ApplyResources(this.TsSharpen, "TsSharpen");
            this.TsSharpen.Name = "TsSharpen";
            this.TsSharpen.Click += new System.EventHandler(this.TsSharpen_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // tsReset
            // 
            this.tsReset.Name = "tsReset";
            resources.ApplyResources(this.tsReset, "tsReset");
            this.tsReset.Click += new System.EventHandler(this.tsReset_Click);
            // 
            // tsdRotate
            // 
            this.tsdRotate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TsRotateLeft,
            this.TsRotateRight,
            this.TsFlip,
            this.TsDeskew,
            this.TsCustomRotation});
            this.tsdRotate.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise;
            resources.ApplyResources(this.tsdRotate, "tsdRotate");
            this.tsdRotate.Name = "tsdRotate";
            this.tsdRotate.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsdRotate.ShowDropDownArrow = false;
            // 
            // TsRotateLeft
            // 
            this.TsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise_small;
            resources.ApplyResources(this.TsRotateLeft, "TsRotateLeft");
            this.TsRotateLeft.Name = "TsRotateLeft";
            this.TsRotateLeft.Click += new System.EventHandler(this.TsRotateLeft_Click);
            // 
            // TsRotateRight
            // 
            this.TsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise_small;
            resources.ApplyResources(this.TsRotateRight, "TsRotateRight");
            this.TsRotateRight.Name = "TsRotateRight";
            this.TsRotateRight.Click += new System.EventHandler(this.TsRotateRight_Click);
            // 
            // TsFlip
            // 
            this.TsFlip.Image = global::NAPS2.Icons.arrow_switch_small;
            resources.ApplyResources(this.TsFlip, "TsFlip");
            this.TsFlip.Name = "TsFlip";
            this.TsFlip.Click += new System.EventHandler(this.TsFlip_Click);
            // 
            // TsDeskew
            // 
            resources.ApplyResources(this.TsDeskew, "TsDeskew");
            this.TsDeskew.Name = "TsDeskew";
            this.TsDeskew.Click += new System.EventHandler(this.TsDeskew_Click);
            // 
            // TsCustomRotation
            // 
            this.TsCustomRotation.Name = "TsCustomRotation";
            resources.ApplyResources(this.TsCustomRotation, "TsCustomRotation");
            this.TsCustomRotation.Click += new System.EventHandler(this.TsCustomRotation_Click);
            // 
            // tsMove
            // 
            this.tsMove.ImageFirst = global::NAPS2.Icons.arrow_up_small;
            this.tsMove.ImageSecond = global::NAPS2.Icons.arrow_down_small;
            this.tsMove.MaxTextWidth = 80;
            this.tsMove.Name = "tsMove";
            this.tsMove.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            resources.ApplyResources(this.tsMove, "tsMove");
            this.tsMove.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.tsMove.ClickFirst += new System.EventHandler(this.tsMove_ClickFirst);
            this.tsMove.ClickSecond += new System.EventHandler(this.tsMove_ClickSecond);
            // 
            // tsdReorder
            // 
            this.tsdReorder.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsInterleave,
            this.tsDeinterleave,
            this.toolStripSeparator12,
            this.tsAltInterleave,
            this.tsAltDeinterleave,
            this.toolStripSeparator1,
            this.tsReverse});
            this.tsdReorder.Image = global::NAPS2.Icons.arrow_refresh;
            resources.ApplyResources(this.tsdReorder, "tsdReorder");
            this.tsdReorder.Name = "tsdReorder";
            this.tsdReorder.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsdReorder.ShowDropDownArrow = false;
            // 
            // tsInterleave
            // 
            this.tsInterleave.Name = "tsInterleave";
            resources.ApplyResources(this.tsInterleave, "tsInterleave");
            this.tsInterleave.Click += new System.EventHandler(this.tsInterleave_Click);
            // 
            // tsDeinterleave
            // 
            this.tsDeinterleave.Name = "tsDeinterleave";
            resources.ApplyResources(this.tsDeinterleave, "tsDeinterleave");
            this.tsDeinterleave.Click += new System.EventHandler(this.tsDeinterleave_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            resources.ApplyResources(this.toolStripSeparator12, "toolStripSeparator12");
            // 
            // tsAltInterleave
            // 
            this.tsAltInterleave.Name = "tsAltInterleave";
            resources.ApplyResources(this.tsAltInterleave, "tsAltInterleave");
            this.tsAltInterleave.Click += new System.EventHandler(this.tsAltInterleave_Click);
            // 
            // tsAltDeinterleave
            // 
            this.tsAltDeinterleave.Name = "tsAltDeinterleave";
            resources.ApplyResources(this.tsAltDeinterleave, "tsAltDeinterleave");
            this.tsAltDeinterleave.Click += new System.EventHandler(this.tsAltDeinterleave_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // tsReverse
            // 
            this.tsReverse.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsReverseAll,
            this.tsReverseSelected});
            this.tsReverse.Name = "tsReverse";
            resources.ApplyResources(this.tsReverse, "tsReverse");
            // 
            // tsReverseAll
            // 
            this.tsReverseAll.Name = "tsReverseAll";
            resources.ApplyResources(this.tsReverseAll, "tsReverseAll");
            this.tsReverseAll.Click += new System.EventHandler(this.tsReverseAll_Click);
            // 
            // tsReverseSelected
            // 
            this.tsReverseSelected.Name = "tsReverseSelected";
            resources.ApplyResources(this.tsReverseSelected, "tsReverseSelected");
            this.tsReverseSelected.Click += new System.EventHandler(this.tsReverseSelected_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // TsDelete
            // 
            this.TsDelete.Image = global::NAPS2.Icons.cross;
            resources.ApplyResources(this.TsDelete, "TsDelete");
            this.TsDelete.Name = "TsDelete";
            this.TsDelete.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.TsDelete.Click += new System.EventHandler(this.TsDelete_Click);
            // 
            // tsClear
            // 
            this.tsClear.Image = global::NAPS2.Icons.cancel;
            resources.ApplyResources(this.tsClear, "tsClear");
            this.tsClear.Name = "tsClear";
            this.tsClear.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsClear.Click += new System.EventHandler(this.tsClear_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.Image = global::NAPS2.Icons.world;
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.toolStripDropDownButton1.ShowDropDownArrow = false;
            // 
            // tsAbout
            // 
            resources.ApplyResources(this.tsAbout, "tsAbout");
            this.tsAbout.Name = "tsAbout";
            this.tsAbout.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsAbout.Click += new System.EventHandler(this.tsAbout_Click);
            // 
            // TsHueSaturation
            // 
            this.TsHueSaturation.Image = global::NAPS2.Icons.color_management;
            resources.ApplyResources(this.TsHueSaturation, "TsHueSaturation");
            this.TsHueSaturation.Name = "TsHueSaturation";
            this.TsHueSaturation.Click += new System.EventHandler(this.TsHueSaturation_Click);
            // 
            // TsBlackWhite
            // 
            this.TsBlackWhite.Image = global::NAPS2.Icons.contrast_high;
            resources.ApplyResources(this.TsBlackWhite, "TsBlackWhite");
            this.TsBlackWhite.Name = "TsBlackWhite";
            this.TsBlackWhite.Click += new System.EventHandler(this.TsBlackWhite_Click);
            // 
            // FDesktop
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.DoubleBuffered = true;
            this.Name = "FDesktop";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.TStrip.ResumeLayout(false);
            this.TStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip TStrip;
        private System.Windows.Forms.ToolStripSplitButton TsScan;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private ThumbnailList ThumbnailList1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private ToolStripDoubleButton tsMove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsProfiles;
        private System.Windows.Forms.ToolStripButton tsAbout;
        private System.Windows.Forms.ToolStripButton TsDelete;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripButton tsImport;
        private System.Windows.Forms.ToolStripSplitButton tsdSavePDF;
        private System.Windows.Forms.ToolStripMenuItem TsSavePDFAll;
        private System.Windows.Forms.ToolStripMenuItem TsSavePDFSelected;
        private System.Windows.Forms.ToolStripDropDownButton tsdRotate;
        private System.Windows.Forms.ToolStripMenuItem TsRotateLeft;
        private System.Windows.Forms.ToolStripMenuItem TsRotateRight;
        private System.Windows.Forms.ToolStripMenuItem TsFlip;
        private System.Windows.Forms.ToolStripDropDownButton tsdReorder;
        private System.Windows.Forms.ToolStripMenuItem tsInterleave;
        private System.Windows.Forms.ToolStripMenuItem tsDeinterleave;
        private System.Windows.Forms.ToolStripSplitButton tsdSaveImages;
        private System.Windows.Forms.ToolStripMenuItem TsSaveImagesAll;
        private System.Windows.Forms.ToolStripMenuItem TsSaveImagesSelected;
        private System.Windows.Forms.ToolStripSplitButton tsdEmailPDF;
        private System.Windows.Forms.ToolStripMenuItem tsEmailPDFAll;
        private System.Windows.Forms.ToolStripMenuItem tsEmailPDFSelected;
        private System.Windows.Forms.ToolStripButton tsOcr;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ctxView;
        private System.Windows.Forms.ToolStripMenuItem ctxSelectAll;
        private System.Windows.Forms.ToolStripMenuItem ctxCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsReverse;
        private System.Windows.Forms.ToolStripMenuItem tsReverseAll;
        private System.Windows.Forms.ToolStripMenuItem tsReverseSelected;
        private System.Windows.Forms.ToolStripMenuItem TsNewProfile;
        private System.Windows.Forms.ToolStripDropDownButton tsdImage;
        private System.Windows.Forms.ToolStripMenuItem tsView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem TsBrightnessContrast;
        private System.Windows.Forms.ToolStripMenuItem TsCrop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem tsReset;
        private System.Windows.Forms.ToolStripMenuItem TsCustomRotation;
        private System.Windows.Forms.ToolStripSeparator ctxSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsPDFSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem tsEmailSettings;
        private System.Windows.Forms.ToolStripMenuItem tsPdfSettings2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.Button BtnZoomIn;
        private System.Windows.Forms.Button BtnZoomOut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem tsImageSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem tsAltInterleave;
        private System.Windows.Forms.ToolStripMenuItem tsAltDeinterleave;
        private System.Windows.Forms.ToolStripMenuItem tsBatchScan;
        private System.Windows.Forms.ToolStripSeparator ctxSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ctxDelete;
        private System.Windows.Forms.Button BtnZoomMouseCatcher;
        private System.Windows.Forms.ToolStripMenuItem ctxPaste;
        private System.Windows.Forms.ToolStripButton tsPrint;
        private System.Windows.Forms.ToolStripMenuItem TsDeskew;
        private System.Windows.Forms.ToolStripMenuItem TsSharpen;
        private System.Windows.Forms.ToolStripMenuItem TsHueSaturation;
        private System.Windows.Forms.ToolStripMenuItem TsBlackWhite;
    }
}

