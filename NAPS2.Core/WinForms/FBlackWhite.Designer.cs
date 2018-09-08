using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FBlackWhite
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBlackWhite));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.txtThreshold = new System.Windows.Forms.TextBox();
            this.tbThreshold = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // txtThreshold
            // 
            resources.ApplyResources(this.txtThreshold, "txtThreshold");
            this.txtThreshold.Name = "txtThreshold";
            this.txtThreshold.TextChanged += new System.EventHandler(this.txtBlackWhite_TextChanged);
            // 
            // tbThreshold
            // 
            resources.ApplyResources(this.tbThreshold, "tbThreshold");
            this.tbThreshold.Maximum = 1000;
            this.tbThreshold.Minimum = -1000;
            this.tbThreshold.Name = "tbThreshold";
            this.tbThreshold.TickFrequency = 200;
            this.tbThreshold.Scroll += new System.EventHandler(this.tbBlackWhite_Scroll);
            // 
            // FBlackWhite
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtThreshold);
            this.Controls.Add(this.tbThreshold);
            this.Controls.Add(this.pictureBox);
            this.Name = "FBlackWhite";
            this.Controls.SetChildIndex(this.pictureBox, 0);
            this.Controls.SetChildIndex(this.tbThreshold, 0);
            this.Controls.SetChildIndex(this.txtThreshold, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtThreshold;
        private System.Windows.Forms.TrackBar tbThreshold;


    }
}
