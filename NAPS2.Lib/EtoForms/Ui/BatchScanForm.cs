using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Config.Model;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.ImportExport;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Exceptions;
using Button = Eto.Forms.Button;
using Control = Eto.Forms.Control;
using DialogResult = Eto.Forms.DialogResult;
using Label = Eto.Forms.Label;
using MessageBox = Eto.Forms.MessageBox;
using MessageBoxButtons = Eto.Forms.MessageBoxButtons;
using RadioButton = Eto.Forms.RadioButton;

namespace NAPS2.EtoForms.Ui;

public class BatchScanForm : EtoDialogBase
{
    private readonly IProfileManager _profileManager;
    private readonly IBatchScanPerformer _batchScanPerformer;
    private readonly ErrorOutput _errorOutput;
    private const string PATCH_CODE_INFO_URL = "https://www.naps2.com/doc/batch-scan#patch-t";

    private readonly Label _status = new() { Text = UiStrings.PressStartWhenReady };
    private readonly Button _start = new() { Text = UiStrings.Start };
    private readonly Button _cancel = new() { Text = UiStrings.Cancel };
    private readonly DropDown _profile = C.DropDown();
    private readonly RadioButton _singleScan;
    private readonly RadioButton _multipleScansPrompt;
    private readonly RadioButton _multipleScansDelay;
    private readonly LayoutVisibility _delayVis = new(false);
    private readonly NumericMaskedTextBox<int> _numberOfScans = new();
    private readonly NumericMaskedTextBox<decimal> _timeBetweenScans = new();
    private readonly RadioButton _load;
    private readonly RadioButton _saveToSingleFile;
    private readonly RadioButton _saveToMultipleFiles;
    private readonly LayoutVisibility _multiVis = new(false);
    private readonly LayoutVisibility _patchTVis = new(false);
    private readonly LayoutVisibility _separatorSheetNewBatchVis = new(false);
    private readonly LayoutVisibility _restartBatchOnErrorVis = new(false);
    private readonly LayoutVisibility _copyToStagingFolderVis = new(false);
    private readonly RadioButton _filePerScan;
    private readonly RadioButton _filePerPage;
    private readonly RadioButton _separateByPatchT;
    // Barcode features
    private readonly RadioButton _separatorSheetNewBatch;
    private readonly RadioButton _includeSeparatorSheet;
    private readonly RadioButton _useBarcodePlaceholder;
    private readonly RadioButton _restartSheetsNumbering;
    private readonly RadioButton _restartBatchOnError;
    private readonly RadioButton _copyToStagingFolder;
    private readonly RadioButton _eraseBatchOnError;
    private readonly RadioButton _useBatchAsFolderName;
    private readonly RadioButton _createPatchTLog;
    private readonly LinkButton _moreInfo = C.UrlLink(PATCH_CODE_INFO_URL, UiStrings.MoreInfo);
    private readonly LayoutVisibility _stagingFolderPathVis = new(false);
    private readonly LayoutVisibility _fileVis = new(false);
    private readonly FolderPath _stagingFolderPath;
    private readonly FilePathWithPlaceholders _filePath;

    private readonly TransactionConfigScope<CommonConfig> _userTransact;
    private readonly Naps2Config _transactionConfig;
    private bool _batchRunning;
    private CancellationTokenSource _cts = new();

    public BatchScanForm(Naps2Config config, DialogHelper dialogHelper, IProfileManager profileManager,
        IBatchScanPerformer batchScanPerformer, ErrorOutput errorOutput)
        : base(config)
    {
        _profileManager = profileManager;
        _batchScanPerformer = batchScanPerformer;
        _errorOutput = errorOutput;
        _singleScan = new RadioButton { Text = UiStrings.SingleScan };
        _multipleScansPrompt = new RadioButton(_singleScan) { Text = UiStrings.MultipleScansPrompt };
        _multipleScansDelay = new RadioButton(_singleScan) { Text = UiStrings.MultipleScansDelay };
        _load = new RadioButton { Text = UiStrings.LoadIn };
        _saveToSingleFile = new RadioButton(_load) { Text = UiStrings.SaveToSingleFile };
        _saveToMultipleFiles = new RadioButton(_load) { Text = UiStrings.SaveToMultipleFiles };
        _filePerScan = new RadioButton { Text = UiStrings.OneFilePerScan };
        _filePerPage = new RadioButton(_filePerScan) { Text = UiStrings.OneFilePerPage };
        _separateByPatchT = new RadioButton(_filePerScan) { Text = UiStrings.SeparateByPatchT };
        // New features
        _separatorSheetNewBatch = new RadioButton() { Text = UiStrings.SeparatorSheetNewBatch };
        _includeSeparatorSheet = new RadioButton() { Text = UiStrings.IncludeSeparatorSheet };
        _useBarcodePlaceholder = new RadioButton() { Text = UiStrings.UseBarcodePlaceholder };
        _restartSheetsNumbering = new RadioButton() { Text = UiStrings.RestartSheetsNumbering };
        _restartBatchOnError = new RadioButton() { Text = UiStrings.RestartBatchOnError };
        _copyToStagingFolder = new RadioButton() { Text = UiStrings.CopyToStagingFolder };
        _eraseBatchOnError = new RadioButton() { Text = UiStrings.EraseBatchOnError };
        _useBatchAsFolderName = new RadioButton() { Text = UiStrings.UseBatchAsFolderName };
        _createPatchTLog = new RadioButton() { Text = UiStrings.CreatePatchTLog };
        _stagingFolderPath = new(this, dialogHelper);
        _filePath = new(this, dialogHelper);

        _start.Click += Start;
        _cancel.Click += Cancel;
        _multipleScansDelay.CheckedChanged += UpdateVisibility;
        _saveToSingleFile.CheckedChanged += UpdateVisibility;
        _saveToMultipleFiles.CheckedChanged += UpdateVisibility;
        _separateByPatchT.CheckedChanged += UpdateVisibility;
        _separatorSheetNewBatch.CheckedChanged += UpdateVisibility;
        _restartBatchOnError.CheckedChanged += UpdateVisibility;
        _copyToStagingFolder.CheckedChanged += UpdateVisibility;

        _userTransact = Config.User.BeginTransaction();
        _transactionConfig = Config.WithTransaction(_userTransact);

        EditProfileCommand = new ActionCommand(EditProfile) { Image = Icons.pencil_small.ToEtoImage() };
        NewProfileCommand = new ActionCommand(NewProfile) { Image = Icons.add_small.ToEtoImage() };
    }

    private void UpdateVisibility(object? sender, EventArgs e)
    {
        _copyToStagingFolderVis.IsVisible = _copyToStagingFolder.Checked;
        _restartBatchOnErrorVis.IsVisible = _restartBatchOnError.Checked;
        _separatorSheetNewBatchVis.IsVisible = _separatorSheetNewBatch.Checked;
        _patchTVis.IsVisible = _separateByPatchT.Checked;
        _delayVis.IsVisible = _multipleScansDelay.Checked;
        _multiVis.IsVisible = _saveToMultipleFiles.Checked;
        _stagingFolderPathVis.IsVisible = _copyToStagingFolder.Checked;
        _fileVis.IsVisible = _saveToSingleFile.Checked || _saveToMultipleFiles.Checked;
        LayoutController.Invalidate();
    }

    public Action<ProcessedImage> ImageCallback { get; set; } = null!;

    private Command NewProfileCommand { get; }

    private Command EditProfileCommand { get; }

    protected override void BuildLayout()
    {
        NewProfileCommand.Enabled =
            !(Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked));
        UpdateUIFromSettings();

        Title = UiStrings.BatchScanFormTitle;
        Icon = new Icon(1f, Icons.application_cascade.ToEtoImage());

        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.Row(
                _status.Scale().AlignCenter().NaturalWidth(200),
                L.OkCancel(
                    _start,
                    _cancel
                )
            ),
            L.GroupBox(
                UiStrings.ScanConfig,
                L.Column(
                    C.Label(UiStrings.ProfileLabel),
                    L.Row(
                        _profile.Scale(),
                        C.Button(EditProfileCommand, ButtonImagePosition.Overlay),
                        C.Button(NewProfileCommand, ButtonImagePosition.Overlay)
                    ),
                    _singleScan,
                    _multipleScansPrompt,
                    _multipleScansDelay,
                    L.Column(
                        C.Label(UiStrings.NumberOfScansLabel),
                        _numberOfScans.Width(50),
                        C.Label(UiStrings.TimeBetweenScansLabel),
                        _timeBetweenScans.Width(50)
                    ).Padding(left: 20).Visible(_delayVis)
                )
            ),
            L.GroupBox(
                UiStrings.Output,
                L.Column(
                    _load,
                    _saveToSingleFile,
                    _saveToMultipleFiles,
                    L.Column(
                        _filePerScan,
                        _filePerPage,
                        _separateByPatchT,
                        L.Column(
                            // New features
                            _separatorSheetNewBatch,
                            L.Column (
                                _includeSeparatorSheet
                            ).Padding(left: 20).Visible(_separatorSheetNewBatchVis),
                            _useBarcodePlaceholder,
                            _restartSheetsNumbering,
                            _restartBatchOnError,
                            L.Column (
                                _copyToStagingFolder,
                                L.Column(
                                    _eraseBatchOnError,
                                    C.Label(UiStrings.StagingFolderLabel),
                                    _stagingFolderPath
                                ).Padding(left: 20).Visible(_copyToStagingFolderVis)
                            ).Padding(left: 20).Visible(_restartBatchOnErrorVis),
                            _useBatchAsFolderName,
                            _createPatchTLog
                        ).Padding(left: 20).Visible(_patchTVis),
                        _moreInfo
                    ).Padding(left: 20).Visible(_multiVis),
                    L.Column(
                        C.Label(UiStrings.FilePathLabel),
                        _filePath
                    ).Visible(_fileVis)
                )
            )
        );
    }


    private void UpdateUIFromSettings()
    {
        UpdateProfiles();

        _singleScan.Checked = _transactionConfig.Get(c => c.BatchSettings.ScanType) == BatchScanType.Single;
        _multipleScansPrompt.Checked =
            _transactionConfig.Get(c => c.BatchSettings.ScanType) == BatchScanType.MultipleWithPrompt;
        _multipleScansDelay.Checked =
            _transactionConfig.Get(c => c.BatchSettings.ScanType) == BatchScanType.MultipleWithDelay;

        // TODO: Verify culture (+ vaildation ofc)
        _numberOfScans.Text =
            _transactionConfig.Get(c => c.BatchSettings.ScanCount).ToString(CultureInfo.CurrentCulture);
        _timeBetweenScans.Text = _transactionConfig.Get(c => c.BatchSettings.ScanIntervalSeconds)
            .ToString(CultureInfo.CurrentCulture);

        _load.Checked = _transactionConfig.Get(c => c.BatchSettings.OutputType) == BatchOutputType.Load;
        _saveToSingleFile.Checked =
            _transactionConfig.Get(c => c.BatchSettings.OutputType) == BatchOutputType.SingleFile;
        _saveToMultipleFiles.Checked =
            _transactionConfig.Get(c => c.BatchSettings.OutputType) == BatchOutputType.MultipleFiles;

        _filePerScan.Checked = _transactionConfig.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.FilePerScan;
        _filePerPage.Checked = _transactionConfig.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.FilePerPage;
        _separateByPatchT.Checked = _transactionConfig.Get(c => c.BatchSettings.SaveSeparator) == SaveSeparator.PatchT;
        // New features

        _filePath.Text = _transactionConfig.Get(c => c.BatchSettings.SavePath);
    }

    private bool ValidateSettings()
    {
        bool ok = true;

        _userTransact.Set(c => c.BatchSettings.ProfileDisplayName, _profile.SelectedKey);
        if (_profile.SelectedIndex == -1)
        {
            ok = false;
            _profile.Focus();
        }

        _userTransact.Set(c => c.BatchSettings.ScanType, _multipleScansPrompt.Checked
            ? BatchScanType.MultipleWithPrompt
            : _multipleScansDelay.Checked
                ? BatchScanType.MultipleWithDelay
                : BatchScanType.Single);

        if (_multipleScansDelay.Checked)
        {
            if (!int.TryParse(_numberOfScans.Text, out int scanCount) || scanCount <= 0)
            {
                ok = false;
                scanCount = 0;
                _numberOfScans.Focus();
            }
            _userTransact.Set(c => c.BatchSettings.ScanCount, scanCount);

            if (!double.TryParse(_timeBetweenScans.Text, out double scanInterval) || scanInterval < 0)
            {
                ok = false;
                scanInterval = 0;
                _timeBetweenScans.Focus();
            }
            _userTransact.Set(c => c.BatchSettings.ScanIntervalSeconds, scanInterval);
        }

        _userTransact.Set(c => c.BatchSettings.OutputType, _saveToSingleFile.Checked ? BatchOutputType.SingleFile
            : _saveToMultipleFiles.Checked ? BatchOutputType.MultipleFiles
            : BatchOutputType.Load);

        _userTransact.Set(c => c.BatchSettings.SaveSeparator, _filePerScan.Checked ? SaveSeparator.FilePerScan
            : _separateByPatchT.Checked ? SaveSeparator.PatchT
            : SaveSeparator.FilePerPage);

        _userTransact.Set(c => c.BatchSettings.SavePath, _filePath.Text);
        if (_transactionConfig.Get(c => c.BatchSettings.OutputType) != BatchOutputType.Load &&
            string.IsNullOrWhiteSpace(_transactionConfig.Get(c => c.BatchSettings.SavePath)))
        {
            ok = false;
            _filePath.Focus();
        }

        return ok;
    }

    private void UpdateProfiles()
    {
        _profile.Items.Clear();
        _profile.Items.AddRange(_profileManager.Profiles.Select(profile => new ListItem
        {
            Text = profile.DisplayName,
            Key = profile.DisplayName,
            Tag = profile
        }));
        if (!string.IsNullOrEmpty(_transactionConfig.Get(c => c.BatchSettings.ProfileDisplayName)) &&
            _profileManager.Profiles.Any(x =>
                x.DisplayName == _transactionConfig.Get(c => c.BatchSettings.ProfileDisplayName)))
        {
            _profile.SelectedKey = _transactionConfig.Get(c => c.BatchSettings.ProfileDisplayName);
        }
        else if (_profileManager.DefaultProfile != null)
        {
            _profile.SelectedKey = _profileManager.DefaultProfile.DisplayName;
        }
        else
        {
            _profile.SelectedKey = null;
        }
    }

    private void EditProfile()
    {
        var originalProfile = (ScanProfile) ((ListItem) _profile.SelectedValue).Tag;
        if (originalProfile != null)
        {
            var fedit = FormFactory.Create<EditProfileForm>();
            fedit.ScanProfile = originalProfile;
            fedit.ShowModal();
            if (fedit.Result)
            {
                _profileManager.Mutate(new ListMutation<ScanProfile>.ReplaceWith(fedit.ScanProfile),
                    ListSelection.Of(originalProfile));
                _userTransact.Set(c => c.BatchSettings.ProfileDisplayName, fedit.ScanProfile.DisplayName);
                UpdateProfiles();
            }
        }
    }

    private void NewProfile()
    {
        if (!(Config.Get(c => c.NoUserProfiles) && _profileManager.Profiles.Any(x => x.IsLocked)))
        {
            var fedit = FormFactory.Create<EditProfileForm>();
            fedit.ScanProfile = Config.DefaultProfileSettings();
            fedit.ShowModal();
            if (fedit.Result)
            {
                _profileManager.Mutate(new ListMutation<ScanProfile>.Append(fedit.ScanProfile),
                    ListSelection.Empty<ScanProfile>());
                _userTransact.Set(c => c.BatchSettings.ProfileDisplayName, fedit.ScanProfile.DisplayName);
                UpdateProfiles();
            }
        }
    }

    private void Start(object? sender, EventArgs args)
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
        _start.Enabled = false;
        _cancel.Enabled = true;
        _cancel.Text = UiStrings.Cancel;
        EnableDisableSettings(false);

        // Start the batch
        DoBatchScan().AssertNoAwait();

        // Save settings for next time (could also do on form close)
        _userTransact.Commit();
    }

    private void EnableDisableSettings(bool enabled)
    {
        var controls = new Control[]
        {
            _profile, _singleScan, _multipleScansPrompt, _multipleScansDelay, _numberOfScans,
            _timeBetweenScans, _load, _saveToSingleFile, _saveToMultipleFiles, _filePerScan, _filePerPage,
            _separateByPatchT, _moreInfo
        };
        foreach (var control in controls)
        {
            control.Enabled = enabled;
        }
        _filePath.Enabled = enabled;
        EditProfileCommand.Enabled = enabled;
        NewProfileCommand.Enabled = enabled;
    }

    private async Task DoBatchScan()
    {
        try
        {
            await _batchScanPerformer.PerformBatchScan(_transactionConfig.Get(c => c.BatchSettings), this,
                image => Invoker.Current.Invoke(() => ImageCallback(image)), ProgressCallback, _cts.Token);
            Invoker.Current.Invoke(() =>
            {
                _status.Text = _cts.IsCancellationRequested
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
            Invoker.Current.Invoke(() => { _status.Text = MiscResources.BatchStatusError; });
        }
        Invoker.Current.Invoke(() =>
        {
            _batchRunning = false;
            _cts = new CancellationTokenSource();
            _start.Enabled = true;
            _cancel.Enabled = true;
            _cancel.Text = MiscResources.Close;
            EnableDisableSettings(true);
            Focus();
        });
    }

    private void ProgressCallback(string status)
    {
        Invoker.Current.Invoke(() => { _status.Text = status; });
    }

    private void Cancel(object? sender, EventArgs e)
    {
        if (_batchRunning)
        {
            if (MessageBox.Show(MiscResources.ConfirmCancelBatch, MiscResources.CancelBatch, MessageBoxButtons.YesNo,
                    MessageBoxType.Question) == DialogResult.Yes)
            {
                _cts.Cancel();
                _cancel.Enabled = false;
                _status.Text = MiscResources.BatchStatusCancelling;
            }
        }
        else
        {
            Close();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_cts.IsCancellationRequested)
        {
            // Keep dialog open while cancel is in progress to avoid concurrency issues
            e.Cancel = true;
        }
    }
}