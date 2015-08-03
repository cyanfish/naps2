/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class TiffViewer : UserControl
    {
        private readonly Container components = null;

        private Image image;
        private PictureBox pbox;
        private int xzoom;

        private bool isControlKeyDown;

        public TiffViewer()
        {
            InitializeComponent();
        }

        public Image Image
        {
            set
            {
                if (value != null)
                {
                    image = value;
                    Zoom = 100;
                }
                else
                {
                    clearimage();
                    image = null;
                }
            }
        }

        public int ImageWidth
        {
            get
            {
                if (image != null)
                {
                    return image.Width;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int ImageHeight
        {
            get
            {
                if (image != null)
                {
                    return image.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int Zoom
        {
            set
            {
                if (image != null)
                {
                    xzoom = Math.Max(Math.Min(value, 1000), 10);
                    double displayWidth = image.Width * ((double)xzoom / 100);
                    double displayHeight = image.Height * ((double)xzoom / 100) * (image.HorizontalResolution / (double)image.VerticalResolution);
                    pbox.Image = image;
                    pbox.Width = (int)displayWidth;
                    pbox.Height = (int)displayHeight;
                    if (ZoomChanged != null)
                    {
                        ZoomChanged.Invoke(this, new EventArgs());
                    }
                }
            }
            get
            { return xzoom; }
        }

        public event EventHandler<EventArgs> ZoomChanged;

        private void clearimage()
        {
            pbox.Image = null;
            pbox.Width = 1;
            pbox.Height = 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (isControlKeyDown)
            {
                StepZoom(e.Delta / (double)SystemInformation.MouseWheelScrollDelta);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            isControlKeyDown = e.Control;
            switch (e.KeyCode)
            {
                case Keys.OemMinus:
                    if (e.Control)
                    {
                        StepZoom(-1);
                    }
                    break;
                case Keys.Oemplus:
                    if (e.Control)
                    {
                        StepZoom(1);
                    }
                    break;
            }
        }

        public void StepZoom(double steps)
        {
            Zoom = (int)Math.Round(Zoom * Math.Pow(1.2, steps));
        }

        private void TiffViewer_KeyDown(object sender, KeyEventArgs e)
        {
            isControlKeyDown = e.Control;
        }

        private void TiffViewer_KeyUp(object sender, KeyEventArgs e)
        {
            isControlKeyDown = e.Control;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiffViewer));
            this.pbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).BeginInit();
            this.SuspendLayout();
            // 
            // pbox
            // 
            resources.ApplyResources(this.pbox, "pbox");
            this.pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbox.Name = "pbox";
            this.pbox.TabStop = false;
            this.pbox.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // TiffViewer
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.pbox);
            this.Name = "TiffViewer";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TiffViewer_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TiffViewer_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
