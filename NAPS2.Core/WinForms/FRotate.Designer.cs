using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FRotate : IEquatable<FRotate>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FRotate));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.TxtAngle = new System.Windows.Forms.TextBox();
            this.TbAngle = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbAngle)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            resources.ApplyResources(this.PictureBox, "PictureBox");
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.TabStop = false;
            this.PictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox_Paint);
            this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDown);
            this.PictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseMove);
            this.PictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseUp);
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
            // TxtAngle
            // 
            resources.ApplyResources(this.TxtAngle, "TxtAngle");
            this.TxtAngle.Name = "TxtAngle";
            this.TxtAngle.TextChanged += new System.EventHandler(this.TxtAngle_TextChanged);
            // 
            // TbAngle
            // 
            resources.ApplyResources(this.TbAngle, "TbAngle");
            this.TbAngle.Maximum = 1800;
            this.TbAngle.Minimum = -1800;
            this.TbAngle.Name = "TbAngle";
            this.TbAngle.TickFrequency = 450;
            this.TbAngle.Scroll += new System.EventHandler(this.TbAngle_Scroll);
            // 
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FRotate
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TxtAngle);
            this.Controls.Add(this.TbAngle);
            this.Controls.Add(this.PictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FRotate";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FRotate_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbAngle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FRotate other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.TextBox TxtAngle;
        private System.Windows.Forms.TrackBar TbAngle;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
