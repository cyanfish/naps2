using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FBrightnessContrast
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBrightnessContrast));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.txtBrightness = new System.Windows.Forms.TextBox();
            this.tbBrightness = new System.Windows.Forms.TrackBar();
            this.txtContrast = new System.Windows.Forms.TextBox();
            this.tbContrast = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).BeginInit();
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
            // txtBrightness
            // 
            resources.ApplyResources(this.txtBrightness, "txtBrightness");
            this.txtBrightness.Name = "txtBrightness";
            this.txtBrightness.TextChanged += new System.EventHandler(this.txtBrightness_TextChanged);
            // 
            // tbBrightness
            // 
            resources.ApplyResources(this.tbBrightness, "tbBrightness");
            this.tbBrightness.Maximum = 1000;
            this.tbBrightness.Minimum = -1000;
            this.tbBrightness.Name = "tbBrightness";
            this.tbBrightness.TickFrequency = 200;
            this.tbBrightness.Scroll += new System.EventHandler(this.tbBrightness_Scroll);
            // 
            // txtContrast
            // 
            resources.ApplyResources(this.txtContrast, "txtContrast");
            this.txtContrast.Name = "txtContrast";
            this.txtContrast.TextChanged += new System.EventHandler(this.txtContrast_TextChanged);
            // 
            // tbContrast
            // 
            resources.ApplyResources(this.tbContrast, "tbContrast");
            this.tbContrast.Maximum = 1000;
            this.tbContrast.Minimum = -1000;
            this.tbContrast.Name = "tbContrast";
            this.tbContrast.TickFrequency = 200;
            this.tbContrast.Scroll += new System.EventHandler(this.tbContrast_Scroll);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::NAPS2.Icons.weather_sun;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Image = global::NAPS2.Icons.contrast;
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // FBrightnessContrast
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtContrast);
            this.Controls.Add(this.txtBrightness);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbContrast);
            this.Controls.Add(this.tbBrightness);
            this.Name = "FBrightnessContrast";
            this.Controls.SetChildIndex(this.tbBrightness, 0);
            this.Controls.SetChildIndex(this.tbContrast, 0);
            this.Controls.SetChildIndex(this.pictureBox, 0);
            this.Controls.SetChildIndex(this.txtBrightness, 0);
            this.Controls.SetChildIndex(this.txtContrast, 0);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.pictureBox2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtBrightness;
        private System.Windows.Forms.TrackBar tbBrightness;
        private System.Windows.Forms.TextBox txtContrast;
        private System.Windows.Forms.TrackBar tbContrast;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
