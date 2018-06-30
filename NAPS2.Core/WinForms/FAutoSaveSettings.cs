using NAPS2.ImportExport;
using NAPS2.Scan;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FAutoSaveSettings : FormBase
    {
        private readonly DialogHelper dialogHelper;

        private bool result;

        public FAutoSaveSettings(DialogHelper dialogHelper)
        {
            this.dialogHelper = dialogHelper;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            if (ScanProfile.AutoSaveSettings != null)
            {
                TxtFilePath.Text = ScanProfile.AutoSaveSettings.FilePath;
                cbPromptForFilePath.Checked = ScanProfile.AutoSaveSettings.PromptForFilePath;
                cbClearAfterSave.Checked = ScanProfile.AutoSaveSettings.ClearImagesAfterSaving;
                if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.FilePerScan)
                {
                    rdFilePerScan.Checked = true;
                }
                else if (ScanProfile.AutoSaveSettings.Separator == SaveSeparator.PatchT)
                {
                    rdSeparateByPatchT.Checked = true;
                }
                else
                {
                    rdFilePerPage.Checked = true;
                }
            }

            new LayoutManager(this)
                .Bind(TxtFilePath)
                    .WidthToForm()
                .Bind(BtnChooseFolder, BtnOK, BtnCancel)
                    .RightToForm()
                .Activate();
        }

        public bool Result => result;

        public ScanProfile ScanProfile { get; set; }

        private void SaveSettings()
        {
            ScanProfile.AutoSaveSettings = new AutoSaveSettings
            {
                FilePath = TxtFilePath.Text,
                PromptForFilePath = cbPromptForFilePath.Checked,
                ClearImagesAfterSaving = cbClearAfterSave.Checked,
                Separator = rdFilePerScan.Checked ? SaveSeparator.FilePerScan
                          : rdSeparateByPatchT.Checked ? SaveSeparator.PatchT
                          : SaveSeparator.FilePerPage
            };
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFilePath.Text) && !cbPromptForFilePath.Checked)
            {
                TxtFilePath.Focus();
                return;
            }
            result = true;
            SaveSettings();
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSavePdfOrImage(null, out string savePath))
            {
                TxtFilePath.Text = savePath;
            }
        }

        private void LinkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = TxtFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                TxtFilePath.Text = form.FileName;
            }
        }

        private void LinkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(FBatchScan.PATCH_CODE_INFO_URL);
        }
    }
}