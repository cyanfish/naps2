/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace NAPS2.WinForms
{
    public class FViewer : FormBase
    {
        private readonly Container components = null;
        private TiffViewerCtl tiffViewer1;

        public FViewer()
        {
            InitializeComponent();
        }

        public Image Image
        {
            get { return tiffViewer1.Image; }
            set { tiffViewer1.Image = value; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (tiffViewer1 != null)
                {
                    tiffViewer1.Image.Dispose();
                    tiffViewer1.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FViewer));
            this.tiffViewer1 = new NAPS2.WinForms.TiffViewerCtl();
            this.SuspendLayout();
            // 
            // tiffViewer1
            // 
            resources.ApplyResources(this.tiffViewer1, "tiffViewer1");
            this.tiffViewer1.Image = null;
            this.tiffViewer1.Name = "tiffViewer1";
            // 
            // FViewer
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.tiffViewer1);
            this.Name = "FViewer";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
