using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    partial class FSharpen : ImageForm
    {
        public FSharpen(ImageContext imageContext, ChangeTracker changeTracker, BitmapRenderer bitmapRenderer)
            : base(imageContext, changeTracker, bitmapRenderer)
        {
            InitializeComponent();
            ActiveControl = txtSharpen;
        }

        public SharpenTransform SharpenTransform { get; private set; } = new SharpenTransform();

        protected override IEnumerable<Transform> Transforms => new [] { SharpenTransform };

        protected override PictureBox PictureBox => pictureBox;

        protected override void ResetTransform()
        {
            SharpenTransform = new SharpenTransform();
            tbSharpen.Value = 0;
            txtSharpen.Text = tbSharpen.Value.ToString("G");
        }

        private void UpdateTransform()
        {
            SharpenTransform = new SharpenTransform(tbSharpen.Value);
            UpdatePreviewBox();
        }
        
        private void txtSharpen_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtSharpen.Text, out int value))
            {
                if (value >= tbSharpen.Minimum && value <= tbSharpen.Maximum)
                {
                    tbSharpen.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbSharpen_Scroll(object sender, EventArgs e)
        {
            txtSharpen.Text = tbSharpen.Value.ToString("G");
            UpdateTransform();
        }
    }
}
