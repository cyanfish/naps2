using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Images;

namespace NAPS2.WinForms
{
    public partial class FImageSettings : FormBase
    {
        private readonly DialogHelper dialogHelper;
        private TransactionConfigScope<CommonConfig> userTransact;
        private TransactionConfigScope<CommonConfig> runTransact;
        private ConfigProvider<CommonConfig> transactProvider;

        public FImageSettings(DialogHelper dialogHelper)
        {
            this.dialogHelper = dialogHelper;
            InitializeComponent();
            AddEnumItems<TiffCompression>(cmbTiffCompr);
        }

        protected override void OnLoad(object sender, EventArgs e)
        {
            new LayoutManager(this)
                .Bind(btnRestoreDefaults, btnOK, btnCancel)
                    .BottomToForm()
                .Bind(txtJpegQuality, btnOK, btnCancel, btnChooseFolder)
                    .RightToForm()
                .Bind(txtDefaultFilePath, tbJpegQuality, lblWarning, groupTiff, groupJpeg)
                    .WidthToForm()
                .Activate();

            userTransact = ConfigScopes.User.BeginTransaction();
            runTransact = ConfigScopes.Run.BeginTransaction();
            transactProvider = ConfigProvider.Replace(ConfigScopes.User, userTransact).Replace(ConfigScopes.Run, runTransact);
            UpdateValues();
            UpdateEnabled();
        }

        private void UpdateValues()
        {
            txtDefaultFilePath.Text = transactProvider.Get(c => c.ImageSettings.DefaultFileName);
            cbSkipSavePrompt.Checked = transactProvider.Get(c => c.ImageSettings.SkipSavePrompt);
            txtJpegQuality.Text = transactProvider.Get(c => c.ImageSettings.JpegQuality).ToString(CultureInfo.InvariantCulture);
            cmbTiffCompr.SelectedIndex = (int)transactProvider.Get(c => c.ImageSettings.TiffCompression);
            cbSinglePageTiff.Checked = transactProvider.Get(c => c.ImageSettings.SinglePageTiff);
            cbRememberSettings.Checked = transactProvider.Get(c => c.RememberImageSettings);
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(txtDefaultFilePath.Text);
        }

        private void txtDefaultFilePath_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var imageSettings = new ImageSettings
            {
                DefaultFileName = txtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
                JpegQuality = tbJpegQuality.Value,
                TiffCompression = (TiffCompression)cmbTiffCompr.SelectedIndex,
                SinglePageTiff = cbSinglePageTiff.Checked
            };

            // Clear old run scope
            runTransact.Set(c => c.ImageSettings = new ImageSettings());

            var scope = cbRememberSettings.Checked ? userTransact : runTransact;
            scope.SetAll(new CommonConfig
            {
                ImageSettings = imageSettings
            });

            userTransact.Commit();
            runTransact.Commit();

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            runTransact.Set(c => c.ImageSettings = new ImageSettings());
            userTransact.Set(c => c.ImageSettings = new ImageSettings());
            userTransact.Set(c => c.RememberImageSettings = false);
            UpdateValues();
            UpdateEnabled();
        }

        private void tbJpegQuality_Scroll(object sender, EventArgs e)
        {
            txtJpegQuality.Text = tbJpegQuality.Value.ToString("G");
        }

        private void txtJpegQuality_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtJpegQuality.Text, out int value))
            {
                if (value >= tbJpegQuality.Minimum && value <= tbJpegQuality.Maximum)
                {
                    tbJpegQuality.Value = value;
                }
            }
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtDefaultFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtDefaultFilePath.Text = form.FileName;
            }
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSaveImage(txtDefaultFilePath.Text, out string savePath))
            {
                txtDefaultFilePath.Text = savePath;
            }
        }
    }
}
