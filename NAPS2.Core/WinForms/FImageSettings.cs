using NAPS2.Config;
using NAPS2.ImportExport.Images;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FImageSettings : FormBase
    {
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IUserConfigManager userConfigManager;
        private readonly DialogHelper dialogHelper;

        public FImageSettings(ImageSettingsContainer imageSettingsContainer, IUserConfigManager userConfigManager, DialogHelper dialogHelper)
        {
            this.imageSettingsContainer = imageSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.dialogHelper = dialogHelper;
            InitializeComponent();
            AddEnumItems<TiffCompression>(cmbTiffCompr);
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(BtnRestoreDefaults, BtnOK, BtnCancel)
                    .BottomToForm()
                .Bind(TxtJpegQuality, BtnOK, BtnCancel, BtnChooseFolder)
                    .RightToForm()
                .Bind(TxtDefaultFilePath, TbJpegQuality, lblWarning, groupTiff, groupJpeg)
                    .WidthToForm()
                .Activate();

            UpdateValues(imageSettingsContainer.ImageSettings);
            UpdateEnabled();
            cbRememberSettings.Checked = userConfigManager.Config.ImageSettings != null;
        }

        private void UpdateValues(ImageSettings imageSettings)
        {
            TxtDefaultFilePath.Text = imageSettings.DefaultFileName;
            cbSkipSavePrompt.Checked = imageSettings.SkipSavePrompt;
            TxtJpegQuality.Text = imageSettings.JpegQuality.ToString(CultureInfo.InvariantCulture);
            cmbTiffCompr.SelectedIndex = (int)imageSettings.TiffCompression;
            cbSinglePageTiff.Checked = imageSettings.SinglePageTiff;
        }

        private void UpdateEnabled()
        {
            cbSkipSavePrompt.Enabled = Path.IsPathRooted(TxtDefaultFilePath.Text);
        }

        private void TxtDefaultFilePath_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var imageSettings = new ImageSettings
            {
                DefaultFileName = TxtDefaultFilePath.Text,
                SkipSavePrompt = cbSkipSavePrompt.Checked,
                JpegQuality = TbJpegQuality.Value,
                TiffCompression = (TiffCompression)cmbTiffCompr.SelectedIndex,
                SinglePageTiff = cbSinglePageTiff.Checked
            };

            imageSettingsContainer.ImageSettings = imageSettings;
            userConfigManager.Config.ImageSettings = cbRememberSettings.Checked ? imageSettings : null;
            userConfigManager.Save();

            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnRestoreDefaults_Click(object sender, EventArgs e)
        {
            UpdateValues(new ImageSettings());
            cbRememberSettings.Checked = false;
        }

        private void TbJpegQuality_Scroll(object sender, EventArgs e)
        {
            TxtJpegQuality.Text = TbJpegQuality.Value.ToString("G");
        }

        private void TxtJpegQuality_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(TxtJpegQuality.Text, out int value))
            {
                if (value >= TbJpegQuality.Minimum && value <= TbJpegQuality.Maximum)
                {
                    TbJpegQuality.Value = value;
                }
            }
        }

        private void LinkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = TxtDefaultFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                TxtDefaultFilePath.Text = form.FileName;
            }
        }

        private void BtnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSaveImage(TxtDefaultFilePath.Text, out string savePath))
            {
                TxtDefaultFilePath.Text = savePath;
            }
        }
    }
}