using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FSharpen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSharpen));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.txtSharpen = new System.Windows.Forms.TextBox();
            this.tbSharpen = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSharpen)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // txtSharpen
            // 
            resources.ApplyResources(this.txtSharpen, "txtSharpen");
            this.txtSharpen.Name = "txtSharpen";
            this.txtSharpen.TextChanged += new System.EventHandler(this.txtSharpen_TextChanged);
            // 
            // tbSharpen
            // 
            resources.ApplyResources(this.tbSharpen, "tbSharpen");
            this.tbSharpen.Maximum = 1000;
            this.tbSharpen.Minimum = -1000;
            this.tbSharpen.Name = "tbSharpen";
            this.tbSharpen.TickFrequency = 200;
            this.tbSharpen.Scroll += new System.EventHandler(this.tbSharpen_Scroll);
            // 
            // FSharpen
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtSharpen);
            this.Controls.Add(this.tbSharpen);
            this.Controls.Add(this.pictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FSharpen";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSharpen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtSharpen;
        private System.Windows.Forms.TrackBar tbSharpen;


    }
}
