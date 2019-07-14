using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch
{
    public class BatchScanPerformer : IBatchScanPerformer
    {
        private readonly IScanPerformer scanPerformer;
        private readonly PdfExporter pdfExporter;
        private readonly IOperationFactory operationFactory;
        private readonly ConfigProvider<PdfSettings> pdfSettingsProvider;
        private readonly OcrEngineManager ocrEngineManager;
        private readonly IFormFactory formFactory;
        private readonly ConfigProvider<CommonConfig> configProvider;
        private readonly IProfileManager profileManager;

        public BatchScanPerformer(IScanPerformer scanPerformer, PdfExporter pdfExporter, IOperationFactory operationFactory, ConfigProvider<PdfSettings> pdfSettingsProvider, OcrEngineManager ocrEngineManager, IFormFactory formFactory, ConfigProvider<CommonConfig> configProvider, IProfileManager profileManager)
        {
            this.scanPerformer = scanPerformer;
            this.pdfExporter = pdfExporter;
            this.operationFactory = operationFactory;
            this.pdfSettingsProvider = pdfSettingsProvider;
            this.ocrEngineManager = ocrEngineManager;
            this.formFactory = formFactory;
            this.configProvider = configProvider;
            this.profileManager = profileManager;
        }

        public async Task PerformBatchScan(ConfigProvider<BatchSettings> settings, FormBase batchForm, Action<ScannedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken)
        {
            var state = new BatchState(scanPerformer, pdfExporter, operationFactory, pdfSettingsProvider, ocrEngineManager, formFactory, configProvider, profileManager)
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
            private readonly IScanPerformer scanPerformer;
            private readonly PdfExporter pdfExporter;
            private readonly IOperationFactory operationFactory;
            private readonly ConfigProvider<PdfSettings> pdfSettingsProvider;
            private readonly OcrEngineManager ocrEngineManager;
            private readonly IFormFactory formFactory;
            private readonly ConfigProvider<CommonConfig> configProvider;
            private readonly IProfileManager profileManager;

            private ScanProfile profile;
            private ScanParams scanParams;
            private List<List<ScannedImage>> scans;

            public BatchState(IScanPerformer scanPerformer, PdfExporter pdfExporter, IOperationFactory operationFactory,
                ConfigProvider<PdfSettings> pdfSettingsProvider, OcrEngineManager ocrEngineManager, IFormFactory formFactory, ConfigProvider<CommonConfig> configProvider, IProfileManager profileManager)
            {
                this.scanPerformer = scanPerformer;
                this.pdfExporter = pdfExporter;
                this.operationFactory = operationFactory;
                this.pdfSettingsProvider = pdfSettingsProvider;
                this.ocrEngineManager = ocrEngineManager;
                this.formFactory = formFactory;
                this.configProvider = configProvider;
                this.profileManager = profileManager;
            }

            public ConfigProvider<BatchSettings> Settings { get; set; }

            public Action<string> ProgressCallback { get; set; }

            public CancellationToken CancelToken { get; set; }

            public FormBase BatchForm { get; set; }

            public Action<ScannedImage> LoadImageCallback { get; set; }

            public async Task Do()
            {
                profile = profileManager.Profiles.First(x => x.DisplayName == Settings.Get(c => c.ProfileDisplayName));
                scanParams = new ScanParams
                {
                    DetectPatchCodes = Settings.Get(c => c.OutputType) == BatchOutputType.MultipleFiles && Settings.Get(c => c.SaveSeparator) == SaveSeparator.PatchT,
                    NoUI = true,
                    NoAutoSave = configProvider.Get(c => c.DisableAutoSave),
                    DoOcr = Settings.Get(c => c.OutputType) == BatchOutputType.Load
                        ? configProvider.Get(c => c.EnableOcr) && configProvider.Get(c => c.OcrAfterScanning) // User configured
                        : configProvider.Get(c => c.EnableOcr) && GetSavePathExtension().ToLower() == ".pdf", // Fully automated
                    OcrParams = configProvider.DefaultOcrParams(),
                    OcrCancelToken = CancelToken,
                    ThumbnailSize = configProvider.Get(c => c.ThumbnailSize)
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
                    scans = new List<List<ScannedImage>>();

                    if (Settings.Get(c => c.ScanType) == BatchScanType.Single)
                    {
                        await InputOneScan(-1);
                    }
                    else if (Settings.Get(c => c.ScanType) == BatchScanType.MultipleWithDelay)
                    {
                        for (int i = 0; i < Settings.Get(c => c.ScanCount); i++)
                        {
                            ProgressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1));
                            if (i != 0)
                            {
                                ThreadSleepWithCancel(TimeSpan.FromSeconds(Settings.Get(c => c.ScanIntervalSeconds)), CancelToken);
                                CancelToken.ThrowIfCancellationRequested();
                            }

                            if (!await InputOneScan(i))
                            {
                                return;
                            }
                        }
                    }
                    else if (Settings.Get(c => c.ScanType) == BatchScanType.MultipleWithPrompt)
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
                var scan = new List<ScannedImage>();
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
                    scans.Add(scan);
                    throw;
                }
                if (scan.Count == 0)
                {
                    // Presume cancelled
                    return false;
                }
                scans.Add(scan);
                return true;
            }

            private async Task DoScan(int scanNumber, List<ScannedImage> scan, int pageNumber)
            {
                var source = await scanPerformer.PerformScan(profile, scanParams, BatchForm.SafeHandle(), CancelToken);
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
                var promptForm = formFactory.Create<FBatchPrompt>();
                promptForm.ScanNumber = scans.Count + 1;
                return promptForm.ShowDialog() == DialogResult.OK;
            }

            private async Task Output()
            {
                ProgressCallback(MiscResources.BatchStatusSaving);

                var placeholders = Placeholders.All.WithDate(DateTime.Now);
                var allImages = scans.SelectMany(x => x).ToList();

                if (Settings.Get(c => c.OutputType) == BatchOutputType.Load)
                {
                    foreach (var image in allImages)
                    {
                        LoadImageCallback(image);
                    }
                }
                else if (Settings.Get(c => c.OutputType) == BatchOutputType.SingleFile)
                {
                    await Save(placeholders, 0, allImages);
                    foreach (var img in allImages)
                    {
                        img.Dispose();
                    }
                }
                else if (Settings.Get(c => c.OutputType) == BatchOutputType.MultipleFiles)
                {
                    int i = 0;
                    foreach (var imageList in SaveSeparatorHelper.SeparateScans(scans, Settings.Get(c => c.SaveSeparator)))
                    {
                        await Save(placeholders, i++, imageList);
                        foreach (var img in imageList)
                        {
                            img.Dispose();
                        }
                    }
                }
            }

            private async Task Save(Placeholders placeholders, int i, List<ScannedImage> images)
            {
                if (images.Count == 0)
                {
                    return;
                }
                var subPath = placeholders.Substitute(Settings.Get(c => c.SavePath), true, i);
                if (GetSavePathExtension().ToLower() == ".pdf")
                {
                    if (File.Exists(subPath))
                    {
                        subPath = placeholders.Substitute(subPath, true, 0, 1);
                    }
                    var snapshots = images.Select(x => x.Preserve()).ToList();
                    try
                    {
                        await pdfExporter.Export(subPath, snapshots, pdfSettingsProvider, new OcrContext(configProvider.DefaultOcrParams()), (j, k) => { }, CancelToken);
                    }
                    finally
                    {
                        snapshots.ForEach(s => s.Dispose());
                    }
                }
                else
                {
                    var op = operationFactory.Create<SaveImagesOperation>();
                    op.Start(subPath, placeholders, images, configProvider.Child(c => c.ImageSettings), true);
                    await op.Success;
                }
            }

            private string GetSavePathExtension()
            {
                if (string.IsNullOrEmpty(Settings.Get(c => c.SavePath)))
                {
                    throw new ArgumentException();
                }
                string extension = Path.GetExtension(Settings.Get(c => c.SavePath));
                Debug.Assert(extension != null);
                return extension;
            }
        }
    }
}
