using System;
using System.Drawing;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FError : FormBase
    {
        public FError()
        {
            RestoreFormState = false;
            InitializeComponent();
            AcceptButton = BtnOK;
        }

        public string ErrorMessage { get; set; }

        public string Details { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            lblErrorText.Text = ErrorMessage;
            TxtDetails.Text = Details;
            ShowHideDetails();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawIcon(SystemIcons.Error, 10, 10);
            base.OnPaint(e);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void LinkDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowHideDetails();
        }

        private void ShowHideDetails()
        {
            TxtDetails.Visible = !TxtDetails.Visible;
            Height += (TxtDetails.Height + 27) * (TxtDetails.Visible ? 1 : -1);
            BtnOK.Top += (TxtDetails.Height + 27) * (TxtDetails.Visible ? 1 : -1);
        }
    }
}