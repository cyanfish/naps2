using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FCrop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FCrop));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tbRight = new System.Windows.Forms.TrackBar();
            this.tbLeft = new System.Windows.Forms.TrackBar();
            this.tbTop = new System.Windows.Forms.TrackBar();
            this.tbBottom = new System.Windows.Forms.TrackBar();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // tbRight
            // 
            resources.ApplyResources(this.tbRight, "tbRight");
            this.tbRight.Name = "tbRight";
            this.tbRight.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // tbLeft
            // 
            resources.ApplyResources(this.tbLeft, "tbLeft");
            this.tbLeft.Name = "tbLeft";
            this.tbLeft.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // tbTop
            // 
            resources.ApplyResources(this.tbTop, "tbTop");
            this.tbTop.Name = "tbTop";
            this.tbTop.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // tbBottom
            // 
            resources.ApplyResources(this.tbBottom, "tbBottom");
            this.tbBottom.Name = "tbBottom";
            this.tbBottom.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnRevert
            // 
            resources.ApplyResources(this.btnRevert, "btnRevert");
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.UseVisualStyleBackColor = true;
            // 
            // FCrop
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbBottom);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.tbTop);
            this.Controls.Add(this.tbLeft);
            this.Controls.Add(this.tbRight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FCrop";
            this.Load += new System.EventHandler(this.FCrop_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TrackBar tbRight;
        private System.Windows.Forms.TrackBar tbLeft;
        private System.Windows.Forms.TrackBar tbTop;
        private System.Windows.Forms.TrackBar tbBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;


    }
}
