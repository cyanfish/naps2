using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.WinForms
{
    partial class ThumbnailList : IDisposable
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
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ThumbnailList));
            this.ilThumbnailList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // ilThumbnailList
            // 
            this.ilThumbnailList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            resources.ApplyResources(this.ilThumbnailList, "ilThumbnailList");
            this.ilThumbnailList.TransparentColor = System.Drawing.Color.Transparent;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList ilThumbnailList;
    }
}
