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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.txtBrightness = new System.Windows.Forms.TextBox();
            this.tbBrightness = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
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
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRevert
            // 
            resources.ApplyResources(this.btnRevert, "btnRevert");
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.btnRevert_Click);
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
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
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
            this.pictureBox1.Image = global::NAPS2.Icons.weather_sun;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::NAPS2.Icons.contrast;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // FBrightnessContrast
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtContrast);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtBrightness);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbContrast);
            this.Controls.Add(this.tbBrightness);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FBrightnessContrast";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
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
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.TextBox txtBrightness;
        private System.Windows.Forms.TrackBar tbBrightness;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;
        private System.Windows.Forms.TextBox txtContrast;
        private System.Windows.Forms.TrackBar tbContrast;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
