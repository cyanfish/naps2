using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FBrightnessContrast : IEquatable<FBrightnessContrast>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBrightnessContrast));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.TxtBrightness = new System.Windows.Forms.TextBox();
            this.TbBrightness = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            this.TxtContrast = new System.Windows.Forms.TextBox();
            this.TbContrast = new System.Windows.Forms.TrackBar();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbContrast)).BeginInit();
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
            // TxtBrightness
            // 
            resources.ApplyResources(this.TxtBrightness, "TxtBrightness");
            this.TxtBrightness.Name = "TxtBrightness";
            this.TxtBrightness.TextChanged += new System.EventHandler(this.TxtBrightness_TextChanged);
            // 
            // TbBrightness
            // 
            resources.ApplyResources(this.TbBrightness, "TbBrightness");
            this.TbBrightness.Maximum = 1000;
            this.TbBrightness.Minimum = -1000;
            this.TbBrightness.Name = "TbBrightness";
            this.TbBrightness.TickFrequency = 200;
            this.TbBrightness.Scroll += new System.EventHandler(this.TbBrightness_Scroll);
            // 
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // TxtContrast
            // 
            resources.ApplyResources(this.TxtContrast, "TxtContrast");
            this.TxtContrast.Name = "TxtContrast";
            this.TxtContrast.TextChanged += new System.EventHandler(this.TxtContrast_TextChanged);
            // 
            // TbContrast
            // 
            resources.ApplyResources(this.TbContrast, "TbContrast");
            this.TbContrast.Maximum = 1000;
            this.TbContrast.Minimum = -1000;
            this.TbContrast.Name = "TbContrast";
            this.TbContrast.TickFrequency = 200;
            this.TbContrast.Scroll += new System.EventHandler(this.TbContrast_Scroll);
            // 
            // PictureBox1
            // 
            this.PictureBox1.Image = global::NAPS2.Icons.weather_sun;
            resources.ApplyResources(this.PictureBox1, "PictureBox1");
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.TabStop = false;
            // 
            // PictureBox2
            // 
            this.PictureBox2.Image = global::NAPS2.Icons.contrast;
            resources.ApplyResources(this.PictureBox2, "PictureBox2");
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.TabStop = false;
            // 
            // FBrightnessContrast
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.TxtContrast);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TxtBrightness);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.TbContrast);
            this.Controls.Add(this.TbBrightness);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FBrightnessContrast";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FBrightnessContrast other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.TextBox TxtBrightness;
        private System.Windows.Forms.TrackBar TbBrightness;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;
        private System.Windows.Forms.TextBox TxtContrast;
        private System.Windows.Forms.TrackBar TbContrast;
        private System.Windows.Forms.PictureBox PictureBox1;
        private System.Windows.Forms.PictureBox PictureBox2;
    }
}
