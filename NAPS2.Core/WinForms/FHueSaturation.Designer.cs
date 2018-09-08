using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FHueSaturation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FHueSaturation));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.txtHue = new System.Windows.Forms.TextBox();
            this.tbHue = new System.Windows.Forms.TrackBar();
            this.txtSaturation = new System.Windows.Forms.TextBox();
            this.tbSaturation = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // txtHue
            // 
            resources.ApplyResources(this.txtHue, "txtHue");
            this.txtHue.Name = "txtHue";
            this.txtHue.TextChanged += new System.EventHandler(this.txtHue_TextChanged);
            // 
            // tbHue
            // 
            resources.ApplyResources(this.tbHue, "tbHue");
            this.tbHue.Maximum = 1000;
            this.tbHue.Minimum = -1000;
            this.tbHue.Name = "tbHue";
            this.tbHue.TickFrequency = 200;
            this.tbHue.Scroll += new System.EventHandler(this.tbHue_Scroll);
            // 
            // txtSaturation
            // 
            resources.ApplyResources(this.txtSaturation, "txtSaturation");
            this.txtSaturation.Name = "txtSaturation";
            this.txtSaturation.TextChanged += new System.EventHandler(this.txtSaturation_TextChanged);
            // 
            // tbSaturation
            // 
            resources.ApplyResources(this.tbSaturation, "tbSaturation");
            this.tbSaturation.Maximum = 1000;
            this.tbSaturation.Minimum = -1000;
            this.tbSaturation.Name = "tbSaturation";
            this.tbSaturation.TickFrequency = 200;
            this.tbSaturation.Scroll += new System.EventHandler(this.tbSaturation_Scroll);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::NAPS2.Icons.color_wheel;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Image = global::NAPS2.Icons.color_gradient;
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // FHueSaturation
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtSaturation);
            this.Controls.Add(this.txtHue);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbSaturation);
            this.Controls.Add(this.tbHue);
            this.Name = "FHueSaturation";
            this.Controls.SetChildIndex(this.tbHue, 0);
            this.Controls.SetChildIndex(this.tbSaturation, 0);
            this.Controls.SetChildIndex(this.pictureBox, 0);
            this.Controls.SetChildIndex(this.txtHue, 0);
            this.Controls.SetChildIndex(this.txtSaturation, 0);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.pictureBox2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtHue;
        private System.Windows.Forms.TrackBar tbHue;
        private System.Windows.Forms.TextBox txtSaturation;
        private System.Windows.Forms.TrackBar tbSaturation;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
