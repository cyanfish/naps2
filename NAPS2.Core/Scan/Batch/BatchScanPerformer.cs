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
using NAPS2.Scan.Images;
using NAPS2.Scan.Twain;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch
{
    public class BatchScanPerformer
    {
        private readonly IScanPerformer scanPerformer;
        private readonly IProfileManager profileManager;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly IPdfExporter pdfExporter;
        private readonly IOperationFactory operationFactory;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly OcrManager ocrManager;
        private readonly IFormFactory formFactory;

        public BatchScanPerformer(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOperationFactory operationFactory, PdfSettingsContainer pdfSettingsContainer, OcrManager ocrManager, IFormFactory formFactory)
        {
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.operationFactory = operationFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.ocrManager = ocrManager;
            this.formFactory = formFactory;
        }

        public async Task PerformBatchScan(BatchSettings settings, FormBase batchForm, Action<ScannedImage> imageCallback, Action<string> progressCallback, CancellationToken cancelToken)
        {
            var state = new BatchState(scanPerformer, profileManager, fileNamePlaceholders, pdfExporter, operationFactory, pdfSettingsContainer, ocrManager, formFactory)
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
            private readonly IProfileManager profileManager;
            private readonly FileNamePlaceholders fileNamePlaceholders;
            private readonly IPdfExporter pdfExporter;
            private readonly IOperationFactory operationFactory;
            private readonly PdfSettingsContainer pdfSettingsContainer;
            private readonly OcrManager ocrManager;
            private readonly IFormFactory formFactory;

            private ScanProfile profile;
            private ScanParams scanParams;
            private List<List<ScannedImage>> scans;

            public BatchState(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOperationFactory operationFactory, PdfSettingsContainer pdfSettingsContainer, OcrManager ocrManager, IFormFactory formFactory)
            {
                this.scanPerformer = scanPerformer;
                this.profileManager = profileManager;
                this.fileNamePlaceholders = fileNamePlaceholders;
                this.pdfExporter = pdfExporter;
                this.operationFactory = operationFactory;
                this.pdfSettingsContainer = pdfSettingsContainer;
                this.ocrManager = ocrManager;
                this.formFactory = formFactory;
            }

            public BatchSettings Settings { get; set; }

            public Action<string> ProgressCallback { get; set; }

            public CancellationToken CancelToken { get; set; }

            public FormBase BatchForm { get; set; }

            public Action<ScannedImage> LoadImageCallback { get; set; }

            public async Task Do()
            {
                profile = profileManager.Profiles.First(x => x.DisplayName == Settings.ProfileDisplayName);
                scanParams = new ScanParams
                {
                    DetectPatchCodes = Settings.OutputType == BatchOutputType.MultipleFiles && Settings.SaveSeparator == SaveSeparator.PatchT,
                    NoUI = true,
                    DoOcr = Settings.OutputType == BatchOutputType.Load ? (bool?)null // Use the default behaviour if we don't know what will be done with the images
                        : GetSavePathExtension().ToLower() == ".pdf" && ocrManager.DefaultParams?.LanguageCode != null,
                    OcrCancelToken = CancelToken
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
                await Task.Factory.StartNew(async () =>
                {
                    scans = new List<List<ScannedImage>>();

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
                                ThreadSleepWithCancel(TimeSpan.FromSeconds(Settings.ScanIntervalSeconds), CancelToken);
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
                }, TaskCreationOptions.LongRunning).Unwrap();
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
                await scanPerformer.PerformScan(profile, scanParams, BatchForm, null, image =>
                {
                    scan.Add(image);
                    CancelToken.ThrowIfCancellationRequested();
                    ProgressCallback(scanNumber == -1
                        ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                        : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1));
                }, CancelToken);
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

                var now = DateTime.Now;
                var allImages = scans.SelectMany(x => x).ToList();

                if (Settings.OutputType == BatchOutputType.Load)
                {
                    foreach (var image in allImages)
                    {
                        LoadImageCallback(image);
                    }
                }
                else if (Settings.OutputType == BatchOutputType.SingleFile)
                {
                    await Save(now, 0, allImages);
                    foreach (var img in allImages)
                    {
                        img.Dispose();
                    }
                }
                else if (Settings.OutputType == BatchOutputType.MultipleFiles)
                {
                    int i = 0;
                    foreach (var imageList in SaveSeparatorHelper.SeparateScans(scans, Settings.SaveSeparator))
                    {
                        await Save(now, i++, imageList);
                        foreach (var img in imageList)
                        {
                            img.Dispose();
                        }
                    }
                }
            }

            private async Task Save(DateTime now, int i, List<ScannedImage> images)
            {
                if (images.Count == 0)
                {
                    return;
                }
                var subPath = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now, true, i);
                if (GetSavePathExtension().ToLower() == ".pdf")
                {
                    if (File.Exists(subPath))
                    {
                        subPath = fileNamePlaceholders.SubstitutePlaceholders(subPath, now, true, 0, 1);
                    }
                    var snapshots = images.Select(x => x.Preserve()).ToList();
                    try
                    {
                        await pdfExporter.Export(subPath, snapshots, pdfSettingsContainer.PdfSettings, ocrManager.DefaultParams, (j, k) => { }, CancelToken);
                    }
                    finally
                    {
                        snapshots.ForEach(s => s.Dispose());
                    }
                }
                else
                {
                    var op = operationFactory.Create<SaveImagesOperation>();
                    op.Start(subPath, now, images, true);
                    await op.Success;
                }
            }

            private string GetSavePathExtension()
            {
                if (Settings.SavePath == null)
                {
                    throw new ArgumentException();
                }
                string extension = Path.GetExtension(Settings.SavePath);
                Debug.Assert(extension != null);
                return extension;
            }
        }
    }
}
