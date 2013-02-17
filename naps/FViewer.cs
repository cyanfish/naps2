using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NAPS
{
	public class FViewer : Form
	{
		private TiffViewerCtl.UTiffViewerCtl tiffViewer1;
		private System.ComponentModel.Container components = null;

		public FViewer(Image obrazek)
		{
			InitializeComponent();
			tiffViewer1.Image = obrazek;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
                if (tiffViewer1 != null)
                {
                    tiffViewer1.Image.Dispose();
                    tiffViewer1.Dispose();
                }
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FViewer));
			this.tiffViewer1 = new TiffViewerCtl.UTiffViewerCtl();
			this.SuspendLayout();
			// 
			// tiffViewer1
			// 
			this.tiffViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tiffViewer1.Image = null;
			this.tiffViewer1.Location = new System.Drawing.Point(0, 0);
			this.tiffViewer1.Name = "tiffViewer1";
			this.tiffViewer1.Size = new System.Drawing.Size(656, 654);
			this.tiffViewer1.TabIndex = 0;
			// 
			// FViewer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(656, 654);
			this.Controls.Add(this.tiffViewer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FViewer";
			this.ShowInTaskbar = false;
			this.Text = "Preview";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
