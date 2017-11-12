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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.txtHue = new System.Windows.Forms.TextBox();
            this.tbHue = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
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
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
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
            this.pictureBox1.Image = global::NAPS2.Icons.color_wheel;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::NAPS2.Icons.color_gradient;
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // FHueSaturation
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtSaturation);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtHue);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbSaturation);
            this.Controls.Add(this.tbHue);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FHueSaturation";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
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
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.TextBox txtHue;
        private System.Windows.Forms.TrackBar tbHue;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;
        private System.Windows.Forms.TextBox txtSaturation;
        private System.Windows.Forms.TrackBar tbSaturation;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
