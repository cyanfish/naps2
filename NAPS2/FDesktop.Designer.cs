namespace NAPS2
{
    partial class FDesktop
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
            {
                components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FDesktop));
            this.tStrip = new System.Windows.Forms.ToolStrip();
            this.tsScan = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsSavePDF = new System.Windows.Forms.ToolStripButton();
            this.tsSaveImage = new System.Windows.Forms.ToolStripButton();
            this.tsPDFEmail = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsMoveUp = new System.Windows.Forms.ToolStripButton();
            this.tsMoveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRotateLeft = new System.Windows.Forms.ToolStripButton();
            this.tsRotateRight = new System.Windows.Forms.ToolStripButton();
            this.tsFlip = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsProfiles = new System.Windows.Forms.ToolStripButton();
            this.tsAbout = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.thumbnailList1 = new NAPS2.ThumbnailList();
            this.tStrip.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tStrip
            // 
            this.tStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.tStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.tStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsScan,
            this.tsProfiles,
            this.toolStripSeparator5,
            this.tsSavePDF,
            this.tsSaveImage,
            this.tsPDFEmail,
            this.toolStripSeparator4,
            this.tsMoveUp,
            this.tsMoveDown,
            this.toolStripSeparator1,
            this.tsRotateLeft,
            this.tsRotateRight,
            this.tsFlip,
            this.toolStripSeparator2,
            this.tsDelete,
            this.toolStripSeparator3,
            this.tsAbout});
            this.tStrip.Location = new System.Drawing.Point(3, 0);
            this.tStrip.Name = "tStrip";
            this.tStrip.ShowItemToolTips = false;
            this.tStrip.Size = new System.Drawing.Size(969, 54);
            this.tStrip.TabIndex = 12;
            this.tStrip.Text = "Main toolbar";
            // 
            // tsScan
            // 
            this.tsScan.Image = ((System.Drawing.Image)(resources.GetObject("tsScan.Image")));
            this.tsScan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsScan.Name = "tsScan";
            this.tsScan.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsScan.Size = new System.Drawing.Size(56, 51);
            this.tsScan.Text = "Scan";
            this.tsScan.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsScan.Click += new System.EventHandler(this.tsScan_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 54);
            // 
            // tsSavePDF
            // 
            this.tsSavePDF.Image = global::NAPS2.Icons.file_extension_pdf;
            this.tsSavePDF.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSavePDF.Name = "tsSavePDF";
            this.tsSavePDF.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsSavePDF.Size = new System.Drawing.Size(79, 51);
            this.tsSavePDF.Text = "Save PDF";
            this.tsSavePDF.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsSavePDF.Click += new System.EventHandler(this.tsSavePDF_Click);
            // 
            // tsSaveImage
            // 
            this.tsSaveImage.Image = global::NAPS2.Icons.picture;
            this.tsSaveImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSaveImage.Name = "tsSaveImage";
            this.tsSaveImage.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsSaveImage.Size = new System.Drawing.Size(96, 51);
            this.tsSaveImage.Text = "Save Images";
            this.tsSaveImage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsSaveImage.Click += new System.EventHandler(this.tsSaveImage_Click);
            // 
            // tsPDFEmail
            // 
            this.tsPDFEmail.Image = global::NAPS2.Icons.email_attach;
            this.tsPDFEmail.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsPDFEmail.Name = "tsPDFEmail";
            this.tsPDFEmail.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsPDFEmail.Size = new System.Drawing.Size(84, 51);
            this.tsPDFEmail.Text = "Email PDF";
            this.tsPDFEmail.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsPDFEmail.Click += new System.EventHandler(this.tsPDFEmail_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 54);
            // 
            // tsMoveUp
            // 
            this.tsMoveUp.Image = global::NAPS2.Icons.arrow_up;
            this.tsMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsMoveUp.Name = "tsMoveUp";
            this.tsMoveUp.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsMoveUp.Size = new System.Drawing.Size(79, 51);
            this.tsMoveUp.Text = "Move Up";
            this.tsMoveUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsMoveUp.Click += new System.EventHandler(this.tsMoveUp_Click);
            // 
            // tsMoveDown
            // 
            this.tsMoveDown.Image = global::NAPS2.Icons.arrow_down;
            this.tsMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsMoveDown.Name = "tsMoveDown";
            this.tsMoveDown.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsMoveDown.Size = new System.Drawing.Size(95, 51);
            this.tsMoveDown.Text = "Move Down";
            this.tsMoveDown.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsMoveDown.Click += new System.EventHandler(this.tsMoveDown_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 54);
            // 
            // tsRotateLeft
            // 
            this.tsRotateLeft.Image = global::NAPS2.Icons.arrow_rotate_anticlockwise;
            this.tsRotateLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsRotateLeft.Name = "tsRotateLeft";
            this.tsRotateLeft.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsRotateLeft.Size = new System.Drawing.Size(88, 51);
            this.tsRotateLeft.Text = "Rotate Left";
            this.tsRotateLeft.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsRotateLeft.Click += new System.EventHandler(this.tsRotateLeft_Click);
            // 
            // tsRotateRight
            // 
            this.tsRotateRight.Image = global::NAPS2.Icons.arrow_rotate_clockwise;
            this.tsRotateRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsRotateRight.Name = "tsRotateRight";
            this.tsRotateRight.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsRotateRight.Size = new System.Drawing.Size(96, 51);
            this.tsRotateRight.Text = "Rotate Right";
            this.tsRotateRight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsRotateRight.Click += new System.EventHandler(this.tsRotateRight_Click);
            // 
            // tsFlip
            // 
            this.tsFlip.Image = global::NAPS2.Icons.arrow_switch;
            this.tsFlip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsFlip.Name = "tsFlip";
            this.tsFlip.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsFlip.Size = new System.Drawing.Size(56, 51);
            this.tsFlip.Text = "Flip";
            this.tsFlip.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsFlip.Click += new System.EventHandler(this.tsFlip_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 54);
            // 
            // tsDelete
            // 
            this.tsDelete.Image = global::NAPS2.Icons.cross;
            this.tsDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsDelete.Size = new System.Drawing.Size(64, 51);
            this.tsDelete.Text = "Delete";
            this.tsDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 54);
            // 
            // tsProfiles
            // 
            this.tsProfiles.Image = ((System.Drawing.Image)(resources.GetObject("tsProfiles.Image")));
            this.tsProfiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsProfiles.Name = "tsProfiles";
            this.tsProfiles.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsProfiles.Size = new System.Drawing.Size(70, 51);
            this.tsProfiles.Text = "Profiles";
            this.tsProfiles.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsProfiles.Click += new System.EventHandler(this.tsProfiles_Click);
            // 
            // tsAbout
            // 
            this.tsAbout.Image = ((System.Drawing.Image)(resources.GetObject("tsAbout.Image")));
            this.tsAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsAbout.Name = "tsAbout";
            this.tsAbout.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.tsAbout.Size = new System.Drawing.Size(64, 51);
            this.tsAbout.Text = "About";
            this.tsAbout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsAbout.Click += new System.EventHandler(this.tsAbout_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.thumbnailList1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1014, 472);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1014, 526);
            this.toolStripContainer1.TabIndex = 13;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tStrip);
            // 
            // thumbnailList1
            // 
            this.thumbnailList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thumbnailList1.Location = new System.Drawing.Point(0, 0);
            this.thumbnailList1.Name = "thumbnailList1";
            this.thumbnailList1.Size = new System.Drawing.Size(1014, 472);
            this.thumbnailList1.TabIndex = 7;
            this.thumbnailList1.UseCompatibleStateImageBehavior = false;
            this.thumbnailList1.ItemActivate += new System.EventHandler(this.thumbnailList1_ItemActivate);
            this.thumbnailList1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.thumbnailList1_KeyDown);
            // 
            // FDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1014, 526);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FDesktop";
            this.Text = "Not Another PDF Scanner 2";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tStrip.ResumeLayout(false);
            this.tStrip.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip tStrip;
        private System.Windows.Forms.ToolStripButton tsScan;
        private System.Windows.Forms.ToolStripButton tsSavePDF;
        private System.Windows.Forms.ToolStripButton tsSaveImage;
        private System.Windows.Forms.ToolStripButton tsPDFEmail;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private ThumbnailList thumbnailList1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsMoveUp;
        private System.Windows.Forms.ToolStripButton tsMoveDown;
        private System.Windows.Forms.ToolStripButton tsRotateRight;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsRotateLeft;
        private System.Windows.Forms.ToolStripButton tsFlip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsProfiles;
        private System.Windows.Forms.ToolStripButton tsAbout;
    }
}

