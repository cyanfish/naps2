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
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.Scan.Batch
{
    public class BatchScanPerformer
    {
        private readonly IScanPerformer scanPerformer;
        private readonly IProfileManager profileManager;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly IPdfExporter pdfExporter;
        private readonly ImageSaver imageSaver;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly UserConfigManager userConfigManager;
        private readonly IFormFactory formFactory;

        public BatchScanPerformer(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, ImageSaver imageSaver, PdfSettingsContainer pdfSettingsContainer, UserConfigManager userConfigManager, IFormFactory formFactory)
        {
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.imageSaver = imageSaver;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
            this.formFactory = formFactory;
        }

        public void PerformBatchScan(BatchSettings settings, IWin32Window dialogParent, Action<IScannedImage> imageCallback, Func<string, bool> progressCallback)
        {
            var state = new BatchState(scanPerformer, profileManager, fileNamePlaceholders, pdfExporter, imageSaver, pdfSettingsContainer, userConfigManager, formFactory)
            {
                Settings = settings,
                ProgressCallback = progressCallback,
                DialogParent = dialogParent,
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
            private readonly ImageSaver imageSaver;
            private readonly PdfSettingsContainer pdfSettingsContainer;
            private readonly UserConfigManager userConfigManager;
            private readonly IFormFactory formFactory;

            private ScanProfile profile;
            private ScanParams scanParams;
            private List<List<IScannedImage>> scans;

            public BatchState(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, ImageSaver imageSaver, PdfSettingsContainer pdfSettingsContainer, UserConfigManager userConfigManager, IFormFactory formFactory)
            {
                this.scanPerformer = scanPerformer;
                this.profileManager = profileManager;
                this.fileNamePlaceholders = fileNamePlaceholders;
                this.pdfExporter = pdfExporter;
                this.imageSaver = imageSaver;
                this.pdfSettingsContainer = pdfSettingsContainer;
                this.userConfigManager = userConfigManager;
                this.formFactory = formFactory;
            }

            public BatchSettings Settings { get; set; }

            public Func<string, bool> ProgressCallback { get; set; }

            public IWin32Window DialogParent { get; set; }

            public Action<IScannedImage> LoadImageCallback { get; set; }

            public void Do()
            {
                profile = profileManager.Profiles.First(x => x.DisplayName == Settings.ProfileDisplayName);
                scanParams = new ScanParams
                {
                    DetectPatchCodes = Settings.OutputType == BatchOutputType.MultipleFiles && Settings.SaveSeparator == BatchSaveSeparator.PatchT,
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
                scans = new List<List<IScannedImage>>();

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
                            Thread.Sleep(TimeSpan.FromSeconds(Settings.ScanIntervalSeconds));
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

            private bool InputOneScan(int scanNumber)
            {
                var scan = new List<IScannedImage>();
                int pageNumber = 1;
                if (!ProgressCallback(scanNumber == -1
                    ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                    : string.Format(MiscResources.BatchStatusScanPage, pageNumber++, scanNumber + 1)))
                {
                    return false;
                }
                try
                {
                    scanPerformer.PerformScan(profile, scanParams, DialogParent, image =>
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
                    if (Settings.SaveSeparator == BatchSaveSeparator.FilePerScan)
                    {
                        for (int i = 0; i < scans.Count; i++)
                        {
                            Save(now, i, scans[i]);
                            foreach (var img in scans[i])
                            {
                                img.Dispose();
                            }
                        }
                    }
                    else if (Settings.SaveSeparator == BatchSaveSeparator.FilePerPage)
                    {
                        for (int i = 0; i < allImages.Count; i++)
                        {
                            Save(now, i, new[] { allImages[i] });
                            allImages[i].Dispose();
                        }
                    }
                    else if (Settings.SaveSeparator == BatchSaveSeparator.PatchT)
                    {
                        var images = new List<IScannedImage>();
                        int fileIndex = 0;
                        foreach (IScannedImage img in allImages)
                        {
                            if (img.PatchCode == PatchCode.PatchT)
                            {
                                if (images.Count > 0)
                                {
                                    Save(now, fileIndex++, images);
                                    foreach (var img2 in images)
                                    {
                                        img2.Dispose();
                                    }
                                    images.Clear();
                                }
                            }
                            else
                            {
                                images.Add(img);
                            }
                        }
                        Save(now, fileIndex, images);
                        foreach (var img in images)
                        {
                            img.Dispose();
                        }
                    }
                }
            }

            private void Save(DateTime now, int i, ICollection<IScannedImage> images)
            {
                var subPath = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now, true, i);
                if (GetSavePathExtension().ToLower() == ".pdf")
                {
                    if (File.Exists(subPath))
                    {
                        subPath = fileNamePlaceholders.SubstitutePlaceholders(subPath, now, true, 0, 1);
                    }
                    pdfExporter.Export(subPath, images, pdfSettingsContainer.PdfSettings,
                        userConfigManager.Config.OcrLanguageCode, j => true);
                }
                else
                {
                    // TODO: Verify behavior for TIFF + others
                    imageSaver.SaveImages(subPath, now, images, j => true);
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
