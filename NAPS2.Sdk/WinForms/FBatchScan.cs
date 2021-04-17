using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.Threading;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FBatchScan : FormBase
    {
        public const string PATCH_CODE_INFO_URL = "http://www.naps2.com/doc-batch-scan.html#patch-t";
        
        private readonly IBatchScanPerformer _batchScanPerformer;
        private readonly ErrorOutput _errorOutput;
        private readonly DialogHelper _dialogHelper;
        private readonly IProfileManager _profileManager;
        private TransactionConfigScope<CommonConfig> _userTransact;
        private ConfigProvider<CommonConfig> _transactProvider;

        private bool _batchRunning;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public FBatchScan(IBatchScanPerformer batchScanPerformer, ErrorOutput errorOutput, DialogHelper dialogHelper, IProfileManager profileManager)
        {
            _batchScanPerformer = batchScanPerformer;
            _errorOutput = errorOutput;
            _dialogHelper = dialogHelper;
            _profileManager = profileManager;
            InitializeComponent();

            RestoreFormState = false;
        }

        public Action<ScannedImage> ImageCallback { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(groupboxScanConfig, groupboxOutput,
                      panelSaveSeparator, panelSaveTo, panelSaveType, panelScanDetails, panelScanType,
                      comboProfile, txtFilePath, lblStatus)
                    .WidthToForm()
                .Bind(btnEditProfile, btnAddProfile, btnStart, btnCancel, btnChooseFolder)
                    .RightToForm()
                .Activate();

            btnAddProfile.Enabled = !(ConfigProvider.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));

            ConditionalControls.LockHeight(this);

            // TODO: Granular
            _userTransact = ConfigScopes.User.BeginTransaction();
            _transactProvider = ConfigProvider.Replace(ConfigScopes.User, _userTransact);
            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            UpdateProfiles();

            rdSingleScan.Checked = _transactProvider.Get(c => c.BatchSettings.ScanType) == BatchScanType.Single;
            rdMultipleScansPrompt.Checked = _transactProvider.Get(c => c.BatchSettings.ScanType) == BatchScanType.MultipleWithPrompt;
            rdMultipleScansDelay.Checked = _transactProvider.Get(c => c.BatchSettings.ScanType) == BatchScanType.MultipleWithDelay;

            // TODO: Verify culture (+ vaildation ofc)
            txtNumberOfScans.Text = _transactProvider.Get(c => c.BatchSettings.ScanCount).ToString(CultureInfo.CurrentCulture);
            txtTimeBetweenScans.Text = _transactProvider.Get(c => c.BatchSettings.ScanIntervalSeconds).ToString(CultureInfo.CurrentCulture);

            rdLoadIntoNaps2.Checked = _transactProvider.Get(c => c.BatchSettings.OutputType) == BatchOutputType.Load;
            rdSaveToSingleFile.Checked = _transactProvider.Get(c => c.BatchSettings.OutputType) == BatchOutputType.SingleFile;
            rdSaveToMultipleFiles.Checked = _transactProvider.Get(c => c.BatchSettings.OutputType) == BatchOutputType.MultipleFiles;

            rdFilePerScan.Checked = _transactProvider.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.FilePerScan;
            rdFilePerPage.Checked = _transactProvider.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.FilePerPage;
            rdSeparateByPatchT.Checked = _transactProvider.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.PatchT;

            txtFilePath.Text = _transactProvider.Get(c => c.BatchSettings.SavePath);
        }

        private bool ValidateSettings()
        {
            bool ok = true;

            _userTransact.Set(c => c.BatchSettings.ProfileDisplayName = comboProfile.Text);
            if (comboProfile.SelectedIndex == -1)
            {
                ok = false;
                comboProfile.Focus();
            }

            _userTransact.Set(c => c.BatchSettings.ScanType = rdMultipleScansPrompt.Checked ? BatchScanType.MultipleWithPrompt
                                   : rdMultipleScansDelay.Checked ? BatchScanType.MultipleWithDelay
                                   : BatchScanType.Single);

            if (rdMultipleScansDelay.Checked)
            {
                if (!int.TryParse(txtNumberOfScans.Text, out int scanCount) || scanCount <= 0)
                {
                    ok = false;
                    scanCount = 0;
                    txtNumberOfScans.Focus();
                }
                _userTransact.Set(c => c.BatchSettings.ScanCount = scanCount);

                if (!double.TryParse(txtTimeBetweenScans.Text, out double scanInterval) || scanInterval < 0)
                {
                    ok = false;
                    scanInterval = 0;
                    txtTimeBetweenScans.Focus();
                }
                _userTransact.Set(c => c.BatchSettings.ScanIntervalSeconds = scanInterval);
            }

            _userTransact.Set(c => c.BatchSettings.OutputType = rdSaveToSingleFile.Checked ? BatchOutputType.SingleFile
                                     : rdSaveToMultipleFiles.Checked ? BatchOutputType.MultipleFiles
                                     : BatchOutputType.Load);

            _userTransact.Set(c => c.BatchSettings.SaveSeparator = rdFilePerScan.Checked ? SaveSeparator.FilePerScan
                                        : rdSeparateByPatchT.Checked ? SaveSeparator.PatchT
                                        : SaveSeparator.FilePerPage);

            _userTransact.Set(c => c.BatchSettings.SavePath = txtFilePath.Text);
            if (_transactProvider.Get(c => c.BatchSettings.OutputType) != BatchOutputType.Load && string.IsNullOrWhiteSpace(_transactProvider.Get(c => c.BatchSettings.SavePath)))
            {
                ok = false;
                txtFilePath.Focus();
            }

            return ok;
        }

        private void UpdateProfiles()
        {
            comboProfile.Items.Clear();
            comboProfile.Items.AddRange(_profileManager.Profiles.Cast<object>().ToArray());
            if (!string.IsNullOrEmpty(_transactProvider.Get(c => c.BatchSettings.ProfileDisplayName)) &&
                _profileManager.Profiles.Any(x => x.DisplayName == _transactProvider.Get(c => c.BatchSettings.ProfileDisplayName)))
            {
                comboProfile.Text = _transactProvider.Get(c => c.BatchSettings.ProfileDisplayName);
            }
            else if (_profileManager.DefaultProfile != null)
            {
                comboProfile.Text = _profileManager.DefaultProfile.DisplayName;
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

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            if (_dialogHelper.PromptToSavePdfOrImage(null, out string savePath))
            {
                txtFilePath.Text = savePath;
            }
        }

        private void linkPatchCodeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(PATCH_CODE_INFO_URL);
        }

        private void comboProfile_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((ScanProfile)e.ListItem).DisplayName;
        }

        private void btnEditProfile_Click(object sender, EventArgs e)
        {
            var originalProfile = (ScanProfile) comboProfile.SelectedItem;
            if (originalProfile != null)
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = originalProfile;
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    _profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile), ListSelection.Of(originalProfile));
                    _userTransact.Set(c => c.BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName);
                    UpdateProfiles();
                }
            }
        }

        private void btnAddProfile_Click(object sender, EventArgs e)
        {
            if (!(ConfigProvider.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked)))
            {
                var fedit = FormFactory.Create<FEditProfile>();
                fedit.ScanProfile = ConfigProvider.Get(c => c.DefaultProfileSettings);
                fedit.ShowDialog();
                if (fedit.Result)
                {
                    _profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile), ListSelection.Empty<ScanProfile>());
                    _userTransact.Set(c => c.BatchSettings.ProfileDisplayName = fedit.ScanProfile.DisplayName);
                    UpdateProfiles();
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_batchRunning)
            {
                return;
            }
            if (!ValidateSettings())
            {
                return;
            }

            // Update state
            _batchRunning = true;
            _cts = new CancellationTokenSource();

            // Update UI
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            btnCancel.Text = MiscResources.Cancel;
            EnableDisableSettings(false);

            // Start the batch
            DoBatchScan().AssertNoAwait();

            // Save settings for next time (could also do on form close)
            _userTransact.Commit();
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

        private async Task DoBatchScan()
        {
            try
            {
                await _batchScanPerformer.PerformBatchScan(ConfigProvider.Child(c => c.BatchSettings), this,
                    image => SafeInvoke(() => ImageCallback(image)), ProgressCallback, _cts.Token);
                SafeInvoke(() =>
                {
                    lblStatus.Text = _cts.IsCancellationRequested
                        ? MiscResources.BatchStatusCancelled
                        : MiscResources.BatchStatusComplete;
                });
            }
            catch (ScanDriverException ex)
            {
                if (ex is ScanDriverUnknownException)
                {
                    Log.ErrorException("Error in batch scan", ex);
                    _errorOutput.DisplayError(ex.Message, ex);
                }
                else
                {
                    _errorOutput.DisplayError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error in batch scan", ex);
                _errorOutput.DisplayError(MiscResources.BatchError, ex);
                SafeInvoke(() =>
                {
                    lblStatus.Text = MiscResources.BatchStatusError;
                });
            }
            SafeInvoke(() =>
            {
                _batchRunning = false;
                _cts = new CancellationTokenSource();
                btnStart.Enabled = true;
                btnCancel.Enabled = true;
                btnCancel.Text = MiscResources.Close;
                EnableDisableSettings(true);
                Activate();
            });
        }

        private void ProgressCallback(string status)
        {
            SafeInvoke(() =>
            {
                lblStatus.Text = status;
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_batchRunning)
            {
                if (MessageBox.Show(MiscResources.ConfirmCancelBatch, MiscResources.CancelBatch, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _cts.Cancel();
                    btnCancel.Enabled = false;
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
            if (_cts.IsCancellationRequested)
            {
                // Keep dialog open while cancel is in progress to avoid concurrency issues
                e.Cancel = true;
            }
        }

        private void linkPlaceholders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var form = FormFactory.Create<FPlaceholders>();
            form.FileName = txtFilePath.Text;
            if (form.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = form.FileName;
            }
        }
    }
}
