using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace TiffViewer
{
	public class UTiffViewer : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.PictureBox pbox;
		private System.ComponentModel.Container components = null;

        public UTiffViewer()
		{
			InitializeComponent();
		}
		
		private Image obrazek;
		private UInt16 xzoom;

        public Image image
		{
			set
			{
				if (value != null)
                {
					obrazek = value;
					Zoom = 100;
				}
				else
				{
					clearimage();
					obrazek = null;
				}
			}
		}

		public int ImageWidth
		{
			get
			{
				if(obrazek != null)
				{
					return obrazek.Width;
				}
				else
				{
					return 0;
				}
			}
		}

		public UInt16 Zoom
		{
			set
			{
				if (value > 5 && value < 1024 && obrazek != null)
				{
					xzoom = value;
					double novasirka = (double)obrazek.Width * ((double)value / 100);
					double novavyska = 0;
					novavyska = (double)novasirka * ((double)obrazek.Height / (double)obrazek.Width) * ((double)obrazek.HorizontalResolution / (double)obrazek.VerticalResolution);
                    Bitmap result = new Bitmap((int)novasirka, (int)novavyska);
                    Graphics g = Graphics.FromImage(result);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(obrazek, 0, 0, (int)novasirka, (int)novavyska);

                    pbox.Image = result;
					pbox.Width = (int)novasirka;
					pbox.Height = (int)novavyska;
				}
			}
			get
			{return xzoom;}
		}

		private void clearimage()
		{
			pbox.Image = null;
			pbox.Width = 1;
			pbox.Height = 1;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
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
            this.pbox.Location = new System.Drawing.Point(10, 10);
            this.pbox.Name = "pbox";
            this.pbox.Size = new System.Drawing.Size(120, 128);
            this.pbox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbox.TabIndex = 0;
            this.pbox.TabStop = false;
            // 
            // tiffviewer
            // 
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.pbox);
            this.Name = "tiffviewer";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
            this.Size = new System.Drawing.Size(544, 520);
            ((System.ComponentModel.ISupportInitialize)(this.pbox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

	

	}
}
