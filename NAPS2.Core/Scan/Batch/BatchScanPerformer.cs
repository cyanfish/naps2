using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
using NAPS2.Util;
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
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IFormFactory formFactory;

        public BatchScanPerformer(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOperationFactory operationFactory, PdfSettingsContainer pdfSettingsContainer, OcrDependencyManager ocrDependencyManager, IFormFactory formFactory)
        {
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.operationFactory = operationFactory;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.ocrDependencyManager = ocrDependencyManager;
            this.formFactory = formFactory;
        }

        public void PerformBatchScan(BatchSettings settings, FormBase batchForm, Action<ScannedImage> imageCallback, Func<string, bool> progressCallback)
        {
            var state = new BatchState(scanPerformer, profileManager, fileNamePlaceholders, pdfExporter, operationFactory, pdfSettingsContainer, ocrDependencyManager, formFactory)
            {
                Settings = settings,
                ProgressCallback = progressCallback,
                BatchForm = batchForm,
                LoadImageCallback = imageCallback
            };
            state.Do();
        }

        private class BatchState
        {
            private readonly IScanPerformer scanPerformer;
            private readonly IProfileManager profileManager;
            private readonly FileNamePlaceholders fileNamePlaceholders;
            private readonly IPdfExporter pdfExporter;
            private readonly IOperationFactory operationFactory;
            private readonly PdfSettingsContainer pdfSettingsContainer;
            private readonly OcrDependencyManager ocrDependencyManager;
            private readonly IFormFactory formFactory;

            private ScanProfile profile;
            private ScanParams scanParams;
            private List<List<ScannedImage>> scans;

            public BatchState(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOperationFactory operationFactory, PdfSettingsContainer pdfSettingsContainer, OcrDependencyManager ocrDependencyManager, IFormFactory formFactory)
            {
                this.scanPerformer = scanPerformer;
                this.profileManager = profileManager;
                this.fileNamePlaceholders = fileNamePlaceholders;
                this.pdfExporter = pdfExporter;
                this.operationFactory = operationFactory;
                this.pdfSettingsContainer = pdfSettingsContainer;
                this.ocrDependencyManager = ocrDependencyManager;
                this.formFactory = formFactory;
            }

            public BatchSettings Settings { get; set; }

            public Func<string, bool> ProgressCallback { get; set; }

            public FormBase BatchForm { get; set; }

            public Action<ScannedImage> LoadImageCallback { get; set; }

            public void Do()
            {
                profile = profileManager.Profiles.First(x => x.DisplayName == Settings.ProfileDisplayName);
                scanParams = new ScanParams
                {
                    DetectPatchCodes = Settings.OutputType == BatchOutputType.MultipleFiles && Settings.SaveSeparator == SaveSeparator.PatchT,
                    NoUI = true
                };
                try
                {
                    Input();
                }
                catch (Exception)
                {
                    // Save at least some data so it isn't lost
                    Output();
                    throw;
                }
                Output();
            }

            private void Input()
            {
                scans = new List<List<ScannedImage>>();

                if (Settings.ScanType == BatchScanType.Single)
                {
                    if (!InputOneScan(-1))
                    {
                        return;
                    }
                }
                else if (Settings.ScanType == BatchScanType.MultipleWithDelay)
                {
                    for (int i = 0; i < Settings.ScanCount; i++)
                    {
                        if (i != 0)
                        {
                            string status = string.Format(MiscResources.BatchStatusWaitingForScan, i + 1);
                            if (!ThreadSleepWithCancel(TimeSpan.FromSeconds(Settings.ScanIntervalSeconds), TimeSpan.FromSeconds(1),
                                () => ProgressCallback(status)))
                            {
                                return;
                            }
                        }
                        if (!InputOneScan(i))
                        {
                            return;
                        }
                        if (!ProgressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 2)))
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
                        if (!InputOneScan(i++))
                        {
                            return;
                        }
                        if (!ProgressCallback(string.Format(MiscResources.BatchStatusWaitingForScan, i + 1)))
                        {
                            return;
                        }
                    } while (PromptForNextScan());
                }
            }

            private bool ThreadSleepWithCancel(TimeSpan sleepDuration, TimeSpan cancelCheckInterval, Func<bool> cancelCheck)
            {
                while (sleepDuration > TimeSpan.Zero)
                {
                    if (sleepDuration > cancelCheckInterval)
                    {
                        Thread.Sleep(cancelCheckInterval);
                        sleepDuration -= cancelCheckInterval;
                    }
                    else
                    {
                        Thread.Sleep(sleepDuration);
                        sleepDuration = TimeSpan.Zero;
                    }
                    if (!cancelCheck())
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool InputOneScan(int scanNumber)
            {
                var scan = new List<ScannedImage>();
                int pageNumber = 1;
                if (!ProgressCallback(scanNumber == -1
                    ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                    : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1)))
                {
                    return false;
                }
                try
                {
                    if (profile.DriverName == TwainScanDriver.DRIVER_NAME || profile.UseNativeUI)
                    {
                        // Apart from WIA with predefined settings, the actual scan needs to be done on the UI thread
                        BatchForm.SafeInvoke(() => DoScan(scanNumber, scan, pageNumber));
                    }
                    else
                    {
                        DoScan(scanNumber, scan, pageNumber);
                    }
                }
                catch (OperationCanceledException)
                {
                    scans.Add(scan);
                    return false;
                }
                if (scan.Count == 0)
                {
                    // Presume cancelled
                    return false;
                }
                scans.Add(scan);
                return true;
            }

            private void DoScan(int scanNumber, List<ScannedImage> scan, int pageNumber)
            {
                scanPerformer.PerformScan(profile, scanParams, BatchForm, null, image =>
                {
                    scan.Add(image);
                    if (!ProgressCallback(scanNumber == -1
                        ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                        : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1)))
                    {
                        throw new OperationCanceledException();
                    }
                });
            }

            private bool PromptForNextScan()
            {
                var promptForm = formFactory.Create<FBatchPrompt>();
                promptForm.ScanNumber = scans.Count + 1;
                return promptForm.ShowDialog() == DialogResult.OK;
            }

            private void Output()
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
                    Save(now, 0, allImages);
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
                        Save(now, i++, imageList);
                        foreach (var img in imageList)
                        {
                            img.Dispose();
                        }
                    }
                }
            }

            private void Save(DateTime now, int i, List<ScannedImage> images)
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
                    pdfExporter.Export(subPath, images, pdfSettingsContainer.PdfSettings, ocrDependencyManager.DefaultLanguageCode, j => true);
                }
                else
                {
                    var op = operationFactory.Create<SaveImagesOperation>();
                    op.Start(subPath, now, images, true);
                    op.WaitUntilFinished();
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
