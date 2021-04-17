using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.WinForms
{
    partial class FHueSaturation : ImageForm
    {
        public FHueSaturation(ImageContext imageContext, BitmapRenderer bitmapRenderer)
            : base(imageContext, bitmapRenderer)
        {
            InitializeComponent();
            ActiveControl = txtHue;
        }

        public HueTransform HueTransform { get; private set; } = new HueTransform();

        public SaturationTransform SaturationTransform { get; private set; } = new SaturationTransform();

        protected override IEnumerable<Transform> Transforms => new Transform[] { HueTransform, SaturationTransform };

        protected override PictureBox PictureBox => pictureBox;
        
        protected override void ResetTransform()
        {
            HueTransform = new HueTransform();
            SaturationTransform = new SaturationTransform();
            tbHue.Value = 0;
            tbSaturation.Value = 0;
            txtHue.Text = tbHue.Value.ToString("G");
            txtSaturation.Text = tbSaturation.Value.ToString("G");
        }

        private void UpdateTransform()
        {
            HueTransform = new HueTransform(tbHue.Value);
            SaturationTransform = new SaturationTransform(tbSaturation.Value);
            UpdatePreviewBox();
        }
        
        private void txtHue_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtHue.Text, out int value))
            {
                if (value >= tbHue.Minimum && value <= tbHue.Maximum)
                {
                    tbHue.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbHue_Scroll(object sender, EventArgs e)
        {
            txtHue.Text = tbHue.Value.ToString("G");
            UpdateTransform();
        }

        private void txtSaturation_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtSaturation.Text, out int value))
            {
                if (value >= tbSaturation.Minimum && value <= tbSaturation.Maximum)
                {
                    tbSaturation.Value = value;
                }
            }
            UpdateTransform();
        }

        private void tbSaturation_Scroll(object sender, EventArgs e)
        {
            txtSaturation.Text = tbSaturation.Value.ToString("G");
            UpdateTransform();
        }
    }
}
