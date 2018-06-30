using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public partial class FBatchScan : FormBase
    {
        public const string PATCH_CODE_INFO_URL = "http://www.naps2.com/doc-batch-scan.html#patch-t";

        private readonly IProfileManager profileManager;
        private readonly AppConfigManager appConfigManager;
        private readonly IconButtonSizer iconButtonSizer;
        private readonly IScanPerformer scanPerformer;
        private readonly IUserConfigManager userConfigManager;
        private readonly BatchScanPerformer batchScanPerformer;
        private readonly IErrorOutput errorOutput;
        private readonly ThreadFactory threadFactory;
        private readonly DialogHelper dialogHelper;

        private bool batchRunning;
        private bool cancelBatch;
        private Thread batchThread;

        public FBatchScan(IProfileManager profileManager, AppConfigManager appConfigManager, IconButtonSizer iconButtonSizer, IScanPerformer scanPerformer, IUserConfigManager userConfigManager, BatchScanPerformer batchScanPerformer, IErrorOutput errorOutput, ThreadFactory threadFactory, DialogHelper dialogHelper)
        {
            this.profileManager = profileManager;
            this.appConfigManager = appConfigManager;
            this.iconButtonSizer = iconButtonSizer;
            this.scanPerformer = scanPerformer;
            this.userConfigManager = userConfigManager;
            this.batchScanPerformer = batchScanPerformer;
            this.errorOutput = errorOutput;
            this.threadFactory = threadFactory;
            this.dialogHelper = dialogHelper;
            InitializeComponent();

            RestoreFormState = false;
        }

        public Action<ScannedImage> ImageCallback { get; set; }

        private BatchSettings BatchSettings { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(groupboxScanConfig, groupboxOutput,
                      panelSaveSeparator, panelSaveTo, panelSaveType, panelScanDetails, panelScanType,
                      comboProfile, TxtFilePath, lblStatus)
                    .WidthToForm()
                .Bind(BtnEditProfile, BtnAddProfile, BtnStart, BtnCancel, BtnChooseFolder)
                    .RightToForm()
                .Activate();

            BtnAddProfile.Enabled = !(appConfigManager.Config.NoUserProfiles && profileManager.Profiles.Any(x => x.IsLocked));

            ConditionalControls.LockHeight(this);

            BatchSettings = userConfigManager.Config.LastBatchSettings ?? new BatchSettings();
            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            UpdateProfiles();

            rdSingleScan.Checked = BatchSettings.ScanType == BatchScanType.Single;
            rdMultipleScansPrompt.Checked = BatchSettings.ScanType == BatchScanType.MultipleWithPrompt;
            rdMultipleScansDelay.Checked = BatchSettings.ScanType == BatchScanType.MultipleWithDelay;

            // TODO: Verify culture (+ vaildation ofc)
            txtNumberOfScans.Text = BatchSettings.ScanCount.ToString(CultureInfo.CurrentCulture);
            txtTimeBetweenScans.Text = BatchSettings.ScanIntervalSeconds.ToString(CultureInfo.CurrentCulture);

            rdLoadIntoNaps2.Checked = BatchSettings.OutputType == BatchOutputType.Load;
            rdSaveToSingleFile.Checked = BatchSettings.OutputType == BatchOutputType.SingleFile;
            rdSaveToMultipleFiles.Checked = BatchSettings.OutputType == BatchOutputType.MultipleFiles;

            rdFilePerScan.Checked = BatchSettings.SaveSeparator == SaveSeparator.FilePerScan;
            rdFilePerPage.Checked = BatchSettings.SaveSeparator == SaveSeparator.FilePerPage;
            rdSeparateByPatchT.Checked = BatchSettings.SaveSeparator == SaveSeparator.PatchT;

            TxtFilePath.Text = BatchSettings.SavePath;
        }

        private bool ValidateSettings()
        {
            bool ok = true;

            BatchSettings.ProfileDisplayName = comboProfile.Text;
            if (comboProfile.SelectedIndex == -1)
            {
                ok = false;
                comboProfile.Focus();
            }

            BatchSettings.ScanType = rdMultipleScansPrompt.Checked ? BatchScanType.MultipleWithPrompt
                                   : rdMultipleScansDelay.Checked ? BatchScanType.MultipleWithDelay
                                   : BatchScanType.Single;

            if (rdMultipleScansDelay.Checked)
            {
                if (!int.TryParse(txtNumberOfScans.Text, out int scanCount) || scanCount <= 0)
                {
                    ok = false;
                    scanCount = 0;
                    txtNumberOfScans.Focus();
                }
                BatchSettings.ScanCount = scanCount;

                if (!double.TryParse(txtTimeBetweenScans.Text, out double scanInterval) || scanInterval < 0)
                {
                    ok = false;
                    scanInterval = 0;
                    txtTimeBetweenScans.Focus();
                }
                BatchSettings.ScanIntervalSeconds = scanInterval;
            }

            BatchSettings.OutputType = rdSaveToSingleFile.Checked ? BatchOutputType.SingleFile
                                     : rdSaveToMultipleFiles.Checked ? BatchOutputType.MultipleFiles
                                     : BatchOutputType.Load;

            BatchSettings.SaveSeparator = rdFilePerScan.Checked ? SaveSeparator.FilePerScan
                                        : rdSeparateByPatchT.Checked ? SaveSeparator.PatchT
                                        : SaveSeparator.FilePerPage;

            BatchSettings.SavePath = TxtFilePath.Text;
            if (BatchSettings.OutputType != BatchOutputType.Load && string.IsNullOrWhiteSpace(BatchSettings.SavePath))
            {
                ok = false;
                TxtFilePath.Focus();
            }

            return ok;
        }

        private void UpdateProfiles()
        {
            comboProfile.Items.Clear();
            comboProfile.Items.AddRange(profileManager.Profiles.Cast<object>().ToArray());
            if (BatchSettings.ProfileDisplayName != null
                && profileManager.Profiles.Any(x => x.DisplayName == BatchSettings.ProfileDisplayName))
            {
                comboProfile.Text = BatchSettings.ProfileDisplayName;
            }
            else if (profileManager.DefaultProfile != null)
            {
                comboProfile.Text = profileManager.DefaultProfile.DisplayName;
            }
            else
            {
                comboProfile.Text = "";
            }
        }

        private void rdSingleScan_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(rdFilePerScan, !rdSingleScan.Checked && rdSaveToMultipleFiles.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdMultipleScansDelay_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelScanDetails, rdMultipleScansDelay.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdLoadIntoNaps2_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelSaveTo, !rdLoadIntoNaps2.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void rdSaveToMultipleFiles_CheckedChanged(object sender, EventArgs e)
        {
            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(panelSaveSeparator, rdSaveToMultipleFiles.Checked);
            ConditionalControls.SetVisible(rdFilePerScan, !rdSingleScan.Checked && rdSaveToMultipleFiles.Checked);
            ConditionalControls.LockHeight(this);
        }

        private void BtnChooseFolder_Click(object sender, EventArgs e)
        {
            if (dialogHelper.PromptToSavePdfOrImage(null, out string savePath))
            {
                TxtFilePath.Text = savePath;
            }
        }

        private void LinkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(PATCH_CODE_INFO_URL);
        }

        private void comboProfile_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((ScanProfile)e.ListItem).DisplayName;
        }

        private void BtnEditProfile_Click(object sender, EventArgs e)
        {
            if (comboProfile.SelectedItem != null)
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = (ScanProfile)comboProfile.SelectedItem;
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Profiles[comboProfile.SelectedIndex] = fedit.ScanProfile;
                    profileManager.Save();
                    BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName;
                    UpdateProfiles();
                }
            }
        }

        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            if (!(appConfigManager.Config.NoUserProfiles && profileManager.Profiles.Any(x => x.IsLocked)))
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = appConfigManager.Config.DefaultProfileSettings ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    profileManager.Profiles.Add(fedit.ScanProfile);
                    profileManager.Save();
                    BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName;
                    UpdateProfiles();
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (batchRunning)
            {
                return;
            }
            if (!ValidateSettings())
            {
                return;
            }

            // Update state
            batchRunning = true;
            cancelBatch = false;

            // Update UI
            BtnStart.Enabled = false;
            BtnCancel.Enabled = true;
            BtnCancel.Text = MiscResources.Cancel;
            EnableDisableSettings(false);

            // Start the batch
            batchThread = threadFactory.CreateThread(DoBatchScan);
            batchThread.Start();

            // Save settings for next time (could also do on form close)
            userConfigManager.Config.LastBatchSettings = BatchSettings;
            userConfigManager.Save();
        }

        private void EnableDisableSettings(bool enabled)
        {
            EnableDisable(groupboxScanConfig, enabled);
            EnableDisable(groupboxOutput, enabled);
        }

        private void EnableDisable(Control root, bool enabled)
        {
            foreach (Control control in root.Controls)
            {
                if (control.Controls.Count > 0)
                {
                    EnableDisable(control, enabled);
                }
                else
                {
                    control.Enabled = enabled;
                }
            }
        }

        private void DoBatchScan()
        {
            try
            {
                batchScanPerformer.PerformBatchScan(BatchSettings, this,
                    image => SafeInvoke(() => ImageCallback(image)), ProgressCallback());
                SafeInvoke(() =>
                {
                    lblStatus.Text = cancelBatch
                        ? MiscResources.BatchStatusCancelled
                        : MiscResources.BatchStatusComplete;
                });
            }
            catch (ScanDriverException ex)
            {
                if (ex is ScanDriverUnknownException)
                {
                    Log.ErrorException("Error in batch scan", ex);
                    errorOutput.DisplayError(ex.Message, ex);
                }
                else
                {
                    errorOutput.DisplayError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error in batch scan", ex);
                errorOutput.DisplayError(MiscResources.BatchError, ex);
                SafeInvoke(() => lblStatus.Text = MiscResources.BatchStatusError);
            }
            SafeInvoke(() =>
            {
                batchRunning = false;
                cancelBatch = false;
                BtnStart.Enabled = true;
                BtnCancel.Enabled = true;
                BtnCancel.Text = MiscResources.Close;
                EnableDisableSettings(true);
                Activate();
            });
        }

        private Func<string, bool> ProgressCallback()
        {
            return status =>
            {
                if (!cancelBatch)
                {
                    SafeInvoke(() => lblStatus.Text = status);
                }
                return !cancelBatch;
            };
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (batchRunning)
            {
                if (MessageBox.Show(MiscResources.ConfirmCancelBatch, MiscResources.CancelBatch, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cancelBatch = true;
                    BtnCancel.Enabled = false;
                    lblStatus.Text = MiscResources.BatchStatusCancelling;
                }
            }
            else
            {
                Close();
            }
        }

        private void FBatchScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel |= cancelBatch;
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
    }
}