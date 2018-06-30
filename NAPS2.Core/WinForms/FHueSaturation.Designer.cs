using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FHueSaturation : IEquatable<FHueSaturation>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FHueSaturation));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.TxtHue = new System.Windows.Forms.TextBox();
            this.TbHue = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            this.TxtSaturation = new System.Windows.Forms.TextBox();
            this.TbSaturation = new System.Windows.Forms.TrackBar();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.PictureBox, "PictureBox");
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.TabStop = false;
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnRevert
            // 
            resources.ApplyResources(this.BtnRevert, "BtnRevert");
            this.BtnRevert.Name = "BtnRevert";
            this.BtnRevert.UseVisualStyleBackColor = true;
            this.BtnRevert.Click += new System.EventHandler(this.BtnRevert_Click);
            // 
            // TxtHue
            // 
            resources.ApplyResources(this.TxtHue, "TxtHue");
            this.TxtHue.Name = "TxtHue";
            this.TxtHue.TextChanged += new System.EventHandler(this.TxtHue_TextChanged);
            // 
            // TbHue
            // 
            resources.ApplyResources(this.TbHue, "TbHue");
            this.TbHue.Maximum = 1000;
            this.TbHue.Minimum = -1000;
            this.TbHue.Name = "TbHue";
            this.TbHue.TickFrequency = 200;
            this.TbHue.Scroll += new System.EventHandler(this.TbHue_Scroll);
            // 
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // TxtSaturation
            // 
            resources.ApplyResources(this.TxtSaturation, "TxtSaturation");
            this.TxtSaturation.Name = "TxtSaturation";
            this.TxtSaturation.TextChanged += new System.EventHandler(this.TxtSaturation_TextChanged);
            // 
            // TbSaturation
            // 
            resources.ApplyResources(this.TbSaturation, "TbSaturation");
            this.TbSaturation.Maximum = 1000;
            this.TbSaturation.Minimum = -1000;
            this.TbSaturation.Name = "TbSaturation";
            this.TbSaturation.TickFrequency = 200;
            this.TbSaturation.Scroll += new System.EventHandler(this.TbSaturation_Scroll);
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = global::NAPS2.Icons.color_wheel;
            resources.ApplyResources(this.PictureBox1, "PictureBox1");
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.TabStop = false;
            // 
            // PictureBox2
            // 
            this.PictureBox2.Image = global::NAPS2.Icons.color_gradient;
            resources.ApplyResources(this.PictureBox2, "PictureBox2");
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.TabStop = false;
            // 
            // FHueSaturation
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.TxtSaturation);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TxtHue);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.TbSaturation);
            this.Controls.Add(this.TbHue);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FHueSaturation";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FHueSaturation other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.TextBox TxtHue;
        private System.Windows.Forms.TrackBar TbHue;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;
        private System.Windows.Forms.TextBox TxtSaturation;
        private System.Windows.Forms.TrackBar TbSaturation;
        private System.Windows.Forms.PictureBox PictureBox1;
        private System.Windows.Forms.PictureBox PictureBox2;
    }
}
