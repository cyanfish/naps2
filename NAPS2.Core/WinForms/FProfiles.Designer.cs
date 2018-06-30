using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class FProfiles
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FProfiles));
            this.lvProfiles = new System.Windows.Forms.ListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxScan = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxSetDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnEdit = new System.Windows.Forms.Button();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.BtnDone = new System.Windows.Forms.Button();
            this.ilProfileIcons = new NAPS2.WinForms.ILProfileIcons(this.components);
            this.BtnScan = new System.Windows.Forms.Button();
            this.ctxCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvProfiles
            // 
            this.lvProfiles.AllowDrop = true;
            this.lvProfiles.ContextMenuStrip = this.contextMenuStrip;
            this.lvProfiles.HideSelection = false;
            resources.ApplyResources(this.lvProfiles, "lvProfiles");
            this.lvProfiles.MultiSelect = false;
            this.lvProfiles.Name = "lvProfiles";
            this.lvProfiles.UseCompatibleStateImageBehavior = false;
            this.lvProfiles.ItemActivate += new System.EventHandler(this.LvProfiles_ItemActivate);
            this.lvProfiles.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.LvProfiles_ItemDrag);
            this.lvProfiles.SelectedIndexChanged += new System.EventHandler(this.LvProfiles_SelectedIndexChanged);
            this.lvProfiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.LvProfiles_DragDrop);
            this.lvProfiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.LvProfiles_DragEnter);
            this.lvProfiles.DragOver += new System.Windows.Forms.DragEventHandler(this.LvProfiles_DragOver);
            this.lvProfiles.DragLeave += new System.EventHandler(this.LvProfiles_DragLeave);
            this.lvProfiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LvProfiles_KeyDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxScan,
            this.ctxEdit,
            this.ctxSetDefault,
            this.toolStripSeparator2,
            this.ctxCopy,
            this.ctxPaste,
            this.toolStripSeparator1,
            this.ctxDelete});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip_Opening);
            // 
            // ctxScan
            // 
            resources.ApplyResources(this.ctxScan, "ctxScan");
            this.ctxScan.Image = global::NAPS2.Icons.control_play_blue_small;
            this.ctxScan.Name = "ctxScan";
            this.ctxScan.Click += new System.EventHandler(this.CtxScan_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // ctxEdit
            // 
            this.ctxEdit.Image = global::NAPS2.Icons.pencil_small;
            this.ctxEdit.Name = "ctxEdit";
            resources.ApplyResources(this.ctxEdit, "ctxEdit");
            this.ctxEdit.Click += new System.EventHandler(this.CtxEdit_Click);
            // 
            // ctxSetDefault
            // 
            this.ctxSetDefault.Image = global::NAPS2.Icons.accept_small;
            this.ctxSetDefault.Name = "ctxSetDefault";
            resources.ApplyResources(this.ctxSetDefault, "ctxSetDefault");
            this.ctxSetDefault.Click += new System.EventHandler(this.CtxSetDefault_Click);
            // 
            // ctxDelete
            // 
            this.ctxDelete.Image = global::NAPS2.Icons.cross_small;
            this.ctxDelete.Name = "ctxDelete";
            resources.ApplyResources(this.ctxDelete, "ctxDelete");
            this.ctxDelete.Click += new System.EventHandler(this.CtxDelete_Click);
            // 
            // BtnAdd
            // 
            resources.ApplyResources(this.BtnAdd, "BtnAdd");
            this.BtnAdd.Image = global::NAPS2.Icons.add_small;
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnEdit
            // 
            this.BtnEdit.Image = global::NAPS2.Icons.pencil_small;
            resources.ApplyResources(this.BtnEdit, "BtnEdit");
            this.BtnEdit.Name = "BtnEdit";
            this.BtnEdit.UseVisualStyleBackColor = true;
            this.BtnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // BtnDelete
            // 
            this.BtnDelete.Image = global::NAPS2.Icons.cross_small;
            resources.ApplyResources(this.BtnDelete, "BtnDelete");
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // BtnDone
            // 
            resources.ApplyResources(this.BtnDone, "BtnDone");
            this.BtnDone.Name = "BtnDone";
            this.BtnDone.UseVisualStyleBackColor = true;
            this.BtnDone.Click += new System.EventHandler(this.BtnDone_Click);
            // 
            // BtnScan
            // 
            this.BtnScan.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.BtnScan, "BtnScan");
            this.BtnScan.Image = global::NAPS2.Icons.control_play_blue;
            this.BtnScan.Name = "BtnScan";
            this.BtnScan.UseVisualStyleBackColor = true;
            this.BtnScan.Click += new System.EventHandler(this.BtnScan_Click);
            // 
            // ctxCopy
            // 
            this.ctxCopy.Name = "ctxCopy";
            resources.ApplyResources(this.ctxCopy, "ctxCopy");
            this.ctxCopy.Click += new System.EventHandler(this.CtxCopy_Click);
            // 
            // ctxPaste
            // 
            this.ctxPaste.Name = "ctxPaste";
            resources.ApplyResources(this.ctxPaste, "ctxPaste");
            this.ctxPaste.Click += new System.EventHandler(this.CtxPaste_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // FProfiles
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnScan);
            this.Controls.Add(this.BtnDone);
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.BtnEdit);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.lvProfiles);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FProfiles";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvProfiles;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.Button BtnEdit;
        private System.Windows.Forms.Button BtnDelete;
        private System.Windows.Forms.Button BtnDone;
        private ILProfileIcons ilProfileIcons;
        private System.Windows.Forms.Button BtnScan;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ctxScan;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ctxEdit;
        private System.Windows.Forms.ToolStripMenuItem ctxDelete;
        private System.Windows.Forms.ToolStripMenuItem ctxSetDefault;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ctxCopy;
        private System.Windows.Forms.ToolStripMenuItem ctxPaste;
    }
}
