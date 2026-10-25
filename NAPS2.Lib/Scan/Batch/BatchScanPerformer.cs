using System.Threading;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Ocr;
using NAPS2.Pdf;

namespace NAPS2.Scan.Batch;

public class BatchScanPerformer : IBatchScanPerformer
{
    private readonly IScanPerformer _scanPerformer;
    private readonly PdfExporter _pdfExporter;
    private readonly IOperationFactory _operationFactory;
    private readonly IFormFactory _formFactory;
    private readonly Naps2Config _config;
    private readonly IProfileManager _profileManager;
    private readonly ThumbnailController _thumbnailController;
    private readonly NameManager _nameManager;

    public BatchScanPerformer(IScanPerformer scanPerformer, PdfExporter pdfExporter,
        IOperationFactory operationFactory,
        IFormFactory formFactory, Naps2Config config, IProfileManager profileManager,
        ThumbnailController thumbnailController, NameManager nameManager)
    {
        _scanPerformer = scanPerformer;
        _pdfExporter = pdfExporter;
        _operationFactory = operationFactory;
        _formFactory = formFactory;
        _config = config;
        _profileManager = profileManager;
        _thumbnailController = thumbnailController;
        _nameManager = nameManager;
    }

    public async Task PerformBatchScan(BatchSettings settings, IFormBase batchForm,
        Action<ProcessedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken)
    {
        var state = new BatchState(_scanPerformer, _pdfExporter, _operationFactory, _formFactory, _config,
            _profileManager, _thumbnailController, _nameManager, settings, progressCallback, cancelToken, batchForm,
            imageCallback);
        await state.Do();
    }

    private class BatchState
    {
        private readonly IScanPerformer _scanPerformer;
        private readonly PdfExporter _pdfExporter;
        private readonly IOperationFactory _operationFactory;
        private readonly IFormFactory _formFactory;
        private readonly Naps2Config _config;
        private readonly IProfileManager _profileManager;
        private readonly NameManager _nameManager;

        private readonly BatchSettings _settings;
        private readonly Action<string> _progressCallback;
        private readonly CancellationToken _cancelToken;
        private readonly IFormBase _batchForm;
        private readonly Action<ProcessedImage> _loadImageCallback;

        private ScanProfile _profile;
        private ScanParams _scanParams;
        private List<List<ProcessedImage>> _scans;

        public BatchState(IScanPerformer scanPerformer, PdfExporter pdfExporter, IOperationFactory operationFactory,
            IFormFactory formFactory, Naps2Config config, IProfileManager profileManager,
            ThumbnailController thumbnailController, NameManager nameManager, BatchSettings settings,
            Action<string> progressCallback, CancellationToken cancelToken, IFormBase batchForm,
            Action<ProcessedImage> loadImageCallback)
        {
            _scanPerformer = scanPerformer;
            _pdfExporter = pdfExporter;
            _operationFactory = operationFactory;
            _formFactory = formFactory;
            _config = config;
            _profileManager = profileManager;
            _nameManager = nameManager;
            _settings = settings;
            _progressCallback = progressCallback;
            _cancelToken = cancelToken;
            _batchForm = batchForm;
            _loadImageCallback = loadImageCallback;

            _profile = _profileManager.Profiles.First(x => x.DisplayName == _settings.ProfileDisplayName);
            _scanParams = new ScanParams
            {
                DetectPatchT = _settings.OutputType == BatchOutputType.MultipleFiles &&
                               _settings.SaveSeparator == SaveSeparator.PatchT,
                NoUI = true,
                NoAutoSave = _config.Get(c => c.DisableAutoSave),
                OcrParams = _settings.OutputType == BatchOutputType.Load
                    ? _config.OcrAfterScanningParams()
                    : GetSavePathExtension().ToLower() == ".pdf"
                        ? _config.DefaultOcrParams()
                        : OcrParams.Empty,
                OcrCancelToken = _cancelToken,
                ThumbnailSize = thumbnailController.RenderSize
            };
            _scans = [];
        }

        public async Task Do()
        {
            try
            {
                _cancelToken.ThrowIfCancellationRequested();
                await Input();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                _cancelToken.ThrowIfCancellationRequested();
                // Save at least some data so it isn't lost
                await Output();
                throw;
            }

            try
            {
                _cancelToken.ThrowIfCancellationRequested();
                await Output();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task Input()
        {
            await Task.Run(async () =>
            {
                if (_settings.ScanType == BatchScanType.Single)
                {
                    await InputOneScan(-1);
                }
                else if (_settings.ScanType == BatchScanType.MultipleWithDelay)
                {
                    for (int i = 0; i < _settings.ScanCount; i++)
                    {
                        _progressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1));
                        if (i != 0)
                        {
                            ThreadSleepWithCancel(TimeSpan.FromSeconds(_settings.ScanIntervalSeconds),
                                _cancelToken);
                            _cancelToken.ThrowIfCancellationRequested();
                        }

                        if (!await InputOneScan(i))
                        {
                            return;
                        }
                    }
                }
                else if (_settings.ScanType == BatchScanType.MultipleWithPrompt)
                {
                    int i = 0;
                    do
                    {
                        _progressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1));
                        if (!await InputOneScan(i++))
                        {
                            return;
                        }
                        _cancelToken.ThrowIfCancellationRequested();
                    } while (PromptForNextScan());
                }
            });
        }

        private void ThreadSleepWithCancel(TimeSpan sleepDuration, CancellationToken cancelToken)
        {
            cancelToken.WaitHandle.WaitOne(sleepDuration);
        }

        private async Task<bool> InputOneScan(int scanNumber)
        {
            var scan = new List<ProcessedImage>();
            int pageNumber = 1;
            _progressCallback(scanNumber == -1
                ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1));
            _cancelToken.ThrowIfCancellationRequested();
            try
            {
                await DoScan(scanNumber, scan, pageNumber);
            }
            catch (OperationCanceledException)
            {
                _scans.Add(scan);
                throw;
            }
            if (scan.Count == 0)
            {
                // Presume cancelled
                return false;
            }
            _scans.Add(scan);
            return true;
        }

        private async Task DoScan(int scanNumber, List<ProcessedImage> scan, int pageNumber)
        {
            var handle = Invoker.Current.InvokeGet(() => (_batchForm as Window)?.NativeHandle ?? IntPtr.Zero);
            var images =
                _scanPerformer.PerformScan(_profile, _scanParams, handle, _cancelToken);
            await foreach(var image in images)
            {
                scan.Add(image);
                _cancelToken.ThrowIfCancellationRequested();
                _progressCallback(scanNumber == -1
                    ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                    : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1));
            }
        }

        private bool PromptForNextScan()
        {
            return Invoker.Current.InvokeGet(() =>
            {
                var promptForm = _formFactory.Create<BatchPromptForm>();
                promptForm.ScanNumber = _scans.Count + 1;
                promptForm.ShowModal();
                return promptForm.Result;
            });
        }

        private async Task Output()
        {
            _progressCallback(MiscResources.BatchStatusSaving);

            var saveParams = new SaveParams { OverwriteMode = OverwriteMode.Never };
            var allImages = _scans.SelectMany(x => x).ToList();

            if (_settings.OutputType == BatchOutputType.Load)
            {
                foreach (var image in allImages)
                {
                    _loadImageCallback(image);
                }
            }
            else if (_settings.OutputType == BatchOutputType.SingleFile)
            {
                await Save(allImages, saveParams);
                foreach (var img in allImages)
                {
                    img.Dispose();
                }
            }
            else if (_settings.OutputType == BatchOutputType.MultipleFiles)
            {
                var imageLists = SaveSeparatorHelper.SeparateScans(_scans, _settings.SaveSeparator)
                    .Where(x => x.Count > 0).ToList();
                saveParams = saveParams with { BatchCount = imageLists.Count };
                foreach (var (imageList, i) in imageLists.Select((x, i) => (x, i)))
                {
                    await Save(imageList, saveParams with { BatchIndex = i });
                    foreach (var img in imageList)
                    {
                        img.Dispose();
                    }
                }
            }
        }

        private async Task Save(List<ProcessedImage> images, SaveParams saveParams)
        {
            if (images.Count == 0)
            {
                return;
            }
            if (GetSavePathExtension().ToLower() == ".pdf")
            {
                using var reservation =
                    _nameManager.Reserve(_settings.SavePath!, NameContext.FromSaveParams(saveParams));
                try
                {
                    var exportParams = new PdfExportParams(
                        _config.Get(c => c.PdfSettings.Metadata).Substitute(_nameManager, saveParams.DateTime),
                        _config.Get(c => c.PdfSettings.Encryption),
                        _config.Get(c => c.PdfSettings.Compat));
                    await _pdfExporter.Export(reservation.Name, images, exportParams, _config.DefaultOcrParams(),
                        _cancelToken);
                }
                finally
                {
                    foreach (var image in images)
                    {
                        image.Dispose();
                    }
                }
            }
            else
            {
                var op = _operationFactory.Create<SaveImagesOperation>();
                op.Start(_settings.SavePath!, images, _config.Get(c => c.ImageSettings), saveParams);
                await op.Success;
            }
        }

        private string GetSavePathExtension()
        {
            if (string.IsNullOrEmpty(_settings.SavePath))
            {
                throw new ArgumentException();
            }
            return Path.GetExtension(_settings.SavePath)!;
        }
    }
}