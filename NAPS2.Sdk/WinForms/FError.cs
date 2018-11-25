using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FError : FormBase
    {
        public FError()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = btnOK;
        }

        public string ErrorMessage { get; set; }

        public string Details { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblErrorText.Text = ErrorMessage;
            txtDetails.Text = Details;
            ShowHideDetails();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawIcon(SystemIcons.Error, 10, 10);
            base.OnPaint(e);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void linkDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowHideDetails();
        }

        private void ShowHideDetails()
        {
            txtDetails.Visible = !txtDetails.Visible;
            Height += (txtDetails.Height + 27) * (txtDetails.Visible ? 1 : -1);
            btnOK.Top += (txtDetails.Height + 27) * (txtDetails.Visible ? 1 : -1);
        }
    }
}
