using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FSharpen : IEquatable<FSharpen>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSharpen));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.TxtSharpen = new System.Windows.Forms.TextBox();
            this.TbSharpen = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbSharpen)).BeginInit();
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
            // TxtSharpen
            // 
            resources.ApplyResources(this.TxtSharpen, "TxtSharpen");
            this.TxtSharpen.Name = "TxtSharpen";
            this.TxtSharpen.TextChanged += new System.EventHandler(this.TxtSharpen_TextChanged);
            // 
            // TbSharpen
            // 
            resources.ApplyResources(this.TbSharpen, "TbSharpen");
            this.TbSharpen.Maximum = 1000;
            this.TbSharpen.Minimum = -1000;
            this.TbSharpen.Name = "TbSharpen";
            this.TbSharpen.TickFrequency = 200;
            this.TbSharpen.Scroll += new System.EventHandler(this.TbSharpen_Scroll);
            // 
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FSharpen
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TxtSharpen);
            this.Controls.Add(this.TbSharpen);
            this.Controls.Add(this.PictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FSharpen";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FSharpen_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbSharpen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FSharpen other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.TextBox TxtSharpen;
        private System.Windows.Forms.TrackBar TbSharpen;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
