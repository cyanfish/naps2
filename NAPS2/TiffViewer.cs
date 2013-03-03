/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TiffViewer
{
    public class TiffViewer : UserControl
    {
        private readonly Container components = null;

        private Image image;
        private PictureBox pbox;
        private int xzoom;

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
                    var result = new Bitmap((int)displayWidth, (int)displayHeight);
                    Graphics g = Graphics.FromImage(result);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, (int)displayWidth, (int)displayHeight);

                    pbox.Image = result;
                    pbox.Width = (int)displayWidth;
                    pbox.Height = (int)displayHeight;
                }
            }
            get
            { return xzoom; }
        }

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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).BeginInit();
            this.SuspendLayout();
            // 
            // pbox
            // 
            this.pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbox.Location = new System.Drawing.Point(10, 10);
            this.pbox.Name = "pbox";
            this.pbox.Size = new System.Drawing.Size(120, 128);
            this.pbox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbox.TabIndex = 0;
            this.pbox.TabStop = false;
            // 
            // UTiffViewer
            // 
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.pbox);
            this.Name = "UTiffViewer";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.Size = new System.Drawing.Size(544, 520);
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
