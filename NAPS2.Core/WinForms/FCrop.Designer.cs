using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FCrop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FCrop));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.tbRight = new System.Windows.Forms.TrackBar();
            this.tbLeft = new System.Windows.Forms.TrackBar();
            this.tbTop = new System.Windows.Forms.TrackBar();
            this.tbBottom = new System.Windows.Forms.TrackBar();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.checkboxApplyToSelected = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // tbRight
            // 
            resources.ApplyResources(this.tbRight, "tbRight");
            this.tbRight.Name = "tbRight";
            this.tbRight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbRight.Scroll += new System.EventHandler(this.tbRight_Scroll);
            // 
            // tbLeft
            // 
            resources.ApplyResources(this.tbLeft, "tbLeft");
            this.tbLeft.Name = "tbLeft";
            this.tbLeft.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbLeft.Scroll += new System.EventHandler(this.tbLeft_Scroll);
            // 
            // tbTop
            // 
            resources.ApplyResources(this.tbTop, "tbTop");
            this.tbTop.Name = "tbTop";
            this.tbTop.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbTop.Scroll += new System.EventHandler(this.tbTop_Scroll);
            // 
            // tbBottom
            // 
            resources.ApplyResources(this.tbBottom, "tbBottom");
            this.tbBottom.Name = "tbBottom";
            this.tbBottom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbBottom.Scroll += new System.EventHandler(this.tbBottom_Scroll);
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
            // checkboxApplyToSelected
            // 
            resources.ApplyResources(this.checkboxApplyToSelected, "checkboxApplyToSelected");
            this.checkboxApplyToSelected.Name = "checkboxApplyToSelected";
            this.checkboxApplyToSelected.UseVisualStyleBackColor = true;
            // 
            // FCrop
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.checkboxApplyToSelected);
            this.Controls.Add(this.tbRight);
            this.Controls.Add(this.tbBottom);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.tbLeft);
            this.Controls.Add(this.tbTop);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FCrop";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FCrop_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBottom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar tbRight;
        private System.Windows.Forms.TrackBar tbLeft;
        private System.Windows.Forms.TrackBar tbTop;
        private System.Windows.Forms.TrackBar tbBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.CheckBox checkboxApplyToSelected;


    }
}
