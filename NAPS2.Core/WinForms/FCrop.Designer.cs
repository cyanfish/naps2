using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FCrop : IEquatable<FCrop>, IDisposable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FCrop));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.TbRight = new System.Windows.Forms.TrackBar();
            this.TbLeft = new System.Windows.Forms.TrackBar();
            this.TbTop = new System.Windows.Forms.TrackBar();
            this.TbBottom = new System.Windows.Forms.TrackBar();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnRevert = new System.Windows.Forms.Button();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            resources.ApplyResources(this.PictureBox, "PictureBox");
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.TabStop = false;
            this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDown);
            this.PictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseMove);
            // 
            // TbRight
            // 
            resources.ApplyResources(this.TbRight, "TbRight");
            this.TbRight.Name = "TbRight";
            this.TbRight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TbRight.Scroll += new System.EventHandler(this.TbRight_Scroll);
            // 
            // TbLeft
            // 
            resources.ApplyResources(this.TbLeft, "TbLeft");
            this.TbLeft.Name = "TbLeft";
            this.TbLeft.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TbLeft.Scroll += new System.EventHandler(this.TbLeft_Scroll);
            // 
            // TbTop
            // 
            resources.ApplyResources(this.TbTop, "TbTop");
            this.TbTop.Name = "TbTop";
            this.TbTop.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TbTop.Scroll += new System.EventHandler(this.TbTop_Scroll);
            // 
            // TbBottom
            // 
            resources.ApplyResources(this.TbBottom, "TbBottom");
            this.TbBottom.Name = "TbBottom";
            this.TbBottom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TbBottom.Scroll += new System.EventHandler(this.TbBottom_Scroll);
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
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FCrop
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.BtnRevert);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.TbRight);
            this.Controls.Add(this.TbBottom);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.TbLeft);
            this.Controls.Add(this.TbTop);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FCrop";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TbBottom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public bool Equals(FCrop other)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.TrackBar TbRight;
        private System.Windows.Forms.TrackBar TbLeft;
        private System.Windows.Forms.TrackBar TbTop;
        private System.Windows.Forms.TrackBar TbBottom;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnRevert;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
