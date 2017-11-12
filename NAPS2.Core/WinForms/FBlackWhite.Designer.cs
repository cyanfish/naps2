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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.txtThreshold = new System.Windows.Forms.TextBox();
            this.tbThreshold = new System.Windows.Forms.TrackBar();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).BeginInit();
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
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FBlackWhite
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtThreshold);
            this.Controls.Add(this.tbThreshold);
            this.Controls.Add(this.pictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FBlackWhite";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FBlackWhite_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.TextBox txtThreshold;
        private System.Windows.Forms.TrackBar tbThreshold;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
