using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FBlackWhite : IEquatable<FBlackWhite>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FBlackWhite));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.txtThreshold = new System.Windows.Forms.TextBox();
            this.tbThreshold = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).BeginInit();
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
            // txtThreshold
            // 
            resources.ApplyResources(this.txtThreshold, "txtThreshold");
            this.txtThreshold.Name = "txtThreshold";
            this.txtThreshold.TextChanged += new System.EventHandler(this.TxtBlackWhite_TextChanged);
            // 
            // tbThreshold
            // 
            resources.ApplyResources(this.tbThreshold, "tbThreshold");
            this.tbThreshold.Maximum = 1000;
            this.tbThreshold.Minimum = -1000;
            this.tbThreshold.Name = "tbThreshold";
            this.tbThreshold.TickFrequency = 200;
            this.tbThreshold.Scroll += new System.EventHandler(this.TbBlackWhite_Scroll);
            // 
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FBlackWhite
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.txtThreshold);
            this.Controls.Add(this.tbThreshold);
            this.Controls.Add(this.PictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FBlackWhite";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FBlackWhite_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FBlackWhite other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.TextBox txtThreshold;
        private System.Windows.Forms.TrackBar tbThreshold;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
