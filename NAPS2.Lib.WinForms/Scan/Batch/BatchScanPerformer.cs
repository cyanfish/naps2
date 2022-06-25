using System.Threading;
using System.Windows.Forms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Ocr;
using NAPS2.Platform.Windows;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch;

public class BatchScanPerformer : IBatchScanPerformer
{
    private readonly IScanPerformer _scanPerformer;
    private readonly PdfExporter _pdfExporter;
    private readonly IOperationFactory _operationFactory;
    private readonly IFormFactory _formFactory;
    private readonly Naps2Config _config;
    private readonly IProfileManager _profileManager;

    public BatchScanPerformer(IScanPerformer scanPerformer, PdfExporter pdfExporter, IOperationFactory operationFactory,
        IFormFactory formFactory, Naps2Config config, IProfileManager profileManager)
    {
        _scanPerformer = scanPerformer;
        _pdfExporter = pdfExporter;
        _operationFactory = operationFactory;
        _formFactory = formFactory;
        _config = config;
        _profileManager = profileManager;
    }

    public async Task PerformBatchScan(BatchSettings settings, FormBase batchForm,
        Action<ProcessedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken)
    {
        var state = new BatchState(_scanPerformer, _pdfExporter, _operationFactory, _formFactory, _config,
            _profileManager)
        {
            Settings = settings,
            ProgressCallback = progressCallback,
            CancelToken = cancelToken,
            BatchForm = batchForm,
            LoadImageCallback = imageCallback
        };
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

        private ScanProfile _profile;
        private ScanParams _scanParams;
        private List<List<ProcessedImage>> _scans;

        public BatchState(IScanPerformer scanPerformer, PdfExporter pdfExporter, IOperationFactory operationFactory,
            IFormFactory formFactory, Naps2Config config, IProfileManager profileManager)
        {
            _scanPerformer = scanPerformer;
            _pdfExporter = pdfExporter;
            _operationFactory = operationFactory;
            _formFactory = formFactory;
            _config = config;
            _profileManager = profileManager;
        }

        public BatchSettings Settings { get; set; }

        public Action<string> ProgressCallback { get; set; }

        public CancellationToken CancelToken { get; set; }

        public FormBase BatchForm { get; set; }

        public Action<ProcessedImage> LoadImageCallback { get; set; }

        public async Task Do()
        {
            _profile = _profileManager.Profiles.First(x => x.DisplayName == Settings.ProfileDisplayName);
            _scanParams = new ScanParams
            {
                DetectPatchT = Settings.OutputType == BatchOutputType.MultipleFiles &&
                               Settings.SaveSeparator == SaveSeparator.PatchT,
                NoUI = true,
                NoAutoSave = _config.Get(c => c.DisableAutoSave),
                DoOcr = Settings.OutputType == BatchOutputType.Load
                    ? _config.Get(c => c.EnableOcr) && _config.Get(c => c.OcrAfterScanning) // User configured
                    : _config.Get(c => c.EnableOcr) && GetSavePathExtension().ToLower() == ".pdf", // Fully automated
                OcrParams = _config.DefaultOcrParams(),
                OcrCancelToken = CancelToken,
                ThumbnailSize = _config.Get(c => c.ThumbnailSize)
            };

            try
            {
                CancelToken.ThrowIfCancellationRequested();
                await Input();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                CancelToken.ThrowIfCancellationRequested();
                // Save at least some data so it isn't lost
                await Output();
                throw;
            }

            try
            {
                CancelToken.ThrowIfCancellationRequested();
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
                _scans = new List<List<ProcessedImage>>();

                if (Settings.ScanType == BatchScanType.Single)
                {
                    await InputOneScan(-1);
                }
                else if (Settings.ScanType == BatchScanType.MultipleWithDelay)
                {
                    for (int i = 0; i < Settings.ScanCount; i++)
                    {
                        ProgressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1));
                        if (i != 0)
                        {
                            ThreadSleepWithCancel(TimeSpan.FromSeconds(Settings.ScanIntervalSeconds),
                                CancelToken);
                            CancelToken.ThrowIfCancellationRequested();
                        }

                        if (!await InputOneScan(i))
                        {
                            return;
                        }
                    }
                }
                else if (Settings.ScanType == BatchScanType.MultipleWithPrompt)
                {
                    int i = 0;
                    do
                    {
                        ProgressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1));
                        if (!await InputOneScan(i++))
                        {
                            return;
                        }
                        CancelToken.ThrowIfCancellationRequested();
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
            ProgressCallback(scanNumber == -1
                ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1));
            CancelToken.ThrowIfCancellationRequested();
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
            var source = await _scanPerformer.PerformScan(_profile, _scanParams, BatchForm.SafeHandle(), CancelToken);
            await source.ForEach(image =>
            {
                scan.Add(image);
                CancelToken.ThrowIfCancellationRequested();
                ProgressCallback(scanNumber == -1
                    ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                    : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1));
            });
        }

        private bool PromptForNextScan()
        {
            var promptForm = _formFactory.Create<FBatchPrompt>();
            promptForm.ScanNumber = _scans.Count + 1;
            return promptForm.ShowDialog() == DialogResult.OK;
        }

        private async Task Output()
        {
            ProgressCallback(MiscResources.BatchStatusSaving);

            var placeholders = Placeholders.All.WithDate(DateTime.Now);
            var allImages = _scans.SelectMany(x => x).ToList();

            if (Settings.OutputType == BatchOutputType.Load)
            {
                foreach (var image in allImages)
                {
                    LoadImageCallback(image);
                }
            }
            else if (Settings.OutputType == BatchOutputType.SingleFile)
            {
                await Save(placeholders, 0, allImages);
                foreach (var img in allImages)
                {
                    img.Dispose();
                }
            }
            else if (Settings.OutputType == BatchOutputType.MultipleFiles)
            {
                int i = 0;
                foreach (var imageList in SaveSeparatorHelper.SeparateScans(_scans, Settings.SaveSeparator))
                {
                    await Save(placeholders, i++, imageList);
                    foreach (var img in imageList)
                    {
                        img.Dispose();
                    }
                }
            }
        }

        private async Task Save(Placeholders placeholders, int i, List<ProcessedImage> images)
        {
            if (images.Count == 0)
            {
                return;
            }
            var subPath = placeholders.Substitute(Settings.SavePath, true, i);
            if (GetSavePathExtension().ToLower() == ".pdf")
            {
                if (File.Exists(subPath))
                {
                    subPath = placeholders.Substitute(subPath, true, 0, 1);
                }
                // TODO: Make copies of images and dispose
                try
                {
                    // TODO: This is broken due to not accessing the child fields directly
                    var exportParams = new PdfExportParams(
                        _config.Get(c => c.PdfSettings.Metadata),
                        _config.Get(c => c.PdfSettings.Encryption),
                        _config.Get(c => c.PdfSettings.Compat));
                    await _pdfExporter.Export(subPath, images, exportParams, _config.DefaultOcrParams(),
                        (j, k) => { }, CancelToken);
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
                op.Start(subPath, placeholders, images, _config.Get(c => c.ImageSettings), true);
                await op.Success;
            }
        }

        private string GetSavePathExtension()
        {
            if (string.IsNullOrEmpty(Settings.SavePath))
            {
                throw new ArgumentException();
            }
            string extension = Path.GetExtension(Settings.SavePath);
            Debug.Assert(extension != null);
            return extension;
        }
    }
}