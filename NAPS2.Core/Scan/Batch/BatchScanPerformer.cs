using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Images;
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
        private readonly ImageSaver imageSaver;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly UserConfigManager userConfigManager;

        public BatchScanPerformer(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, ImageSaver imageSaver, PdfSettingsContainer pdfSettingsContainer, UserConfigManager userConfigManager)
        {
            this.scanPerformer = scanPerformer;
            this.profileManager = profileManager;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.imageSaver = imageSaver;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.userConfigManager = userConfigManager;
        }

        public void PerformBatchScan(BatchSettings settings, IWin32Window dialogParent, Action<IScannedImage> imageCallback, Func<string, bool> progressCallback)
        {
            var state = new BatchState(scanPerformer, profileManager, fileNamePlaceholders, pdfExporter, imageSaver, pdfSettingsContainer, userConfigManager)
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

            private ExtendedScanSettings profile;
            private List<List<IScannedImage>> scans;

            public BatchState(IScanPerformer scanPerformer, IProfileManager profileManager, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, ImageSaver imageSaver, PdfSettingsContainer pdfSettingsContainer, UserConfigManager userConfigManager)
            {
                this.scanPerformer = scanPerformer;
                this.profileManager = profileManager;
                this.fileNamePlaceholders = fileNamePlaceholders;
                this.pdfExporter = pdfExporter;
                this.imageSaver = imageSaver;
                this.pdfSettingsContainer = pdfSettingsContainer;
                this.userConfigManager = userConfigManager;
            }

            public BatchSettings Settings { get; set; }

            public Func<string, bool> ProgressCallback { get; set; }

            public IWin32Window DialogParent { get; set; }

            public Action<IScannedImage> LoadImageCallback { get; set; }

            public void Do()
            {
                profile = profileManager.Profiles.First(x => x.DisplayName == Settings.ProfileDisplayName);
                Input();
                Output();
            }

            private void Input()
            {
                scans = new List<List<IScannedImage>>();

                if (Settings.ScanType == BatchScanType.Single)
                {
                    InputOneScan(-1);
                }
                else if (Settings.ScanType == BatchScanType.MultipleWithDelay)
                {
                    for (int i = 0; i < Settings.ScanCount; i++)
                    {
                        if (i != 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(Settings.ScanIntervalSeconds));
                        }
                        InputOneScan(i);
                    }
                }
                else if (Settings.ScanType == BatchScanType.MultipleWithPrompt)
                {
                    int i = 0;
                    do
                    {
                        InputOneScan(i++);
                    } while (PromptForNextScan());
                }
            }

            private void InputOneScan(int scanNumber)
            {
                var scan = new List<IScannedImage>();
                int pageNumber = 1;
                ProgressCallback(scanNumber == -1
                    ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                    : string.Format(MiscResources.BatchStatusScanPage, scanNumber + 1, pageNumber++));
                scanPerformer.PerformScan(profile, DialogParent, image =>
                {
                    scan.Add(image);
                    ProgressCallback(scanNumber == -1
                        ? string.Format(MiscResources.BatchStatusPage, pageNumber++)
                        : string.Format(MiscResources.BatchStatusScanPage, scanNumber + 1, pageNumber++));
                });
                scans.Add(scan);
            }

            private bool PromptForNextScan()
            {
                throw new NotImplementedException();
            }

            private void Output()
            {
                var now = DateTime.Now;
                var allImages = scans.SelectMany(x => x).ToList();
                string extension = Path.GetExtension(Settings.SavePath);
                Debug.Assert(extension != null);

                if (Settings.OutputType == BatchOutputType.Load)
                {
                    foreach (var image in allImages)
                    {
                        LoadImageCallback(image);
                    }
                }
                else if (Settings.OutputType == BatchOutputType.SingleFile)
                {
                    if (extension.ToLower() == ".pdf")
                    {
                        var subPath = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now);
                        pdfExporter.Export(subPath, allImages, pdfSettingsContainer.PdfSettings,
                            userConfigManager.Config.OcrLanguageCode, i => true);
                        // TODO: Add an actual progress callback
                    }
                    else
                    {
                        imageSaver.SaveImages(Settings.SavePath, now, allImages, i => true);
                    }
                }
                else if (Settings.OutputType == BatchOutputType.MultipleFiles)
                {
                    if (Settings.SaveSeparator == BatchSaveSeparator.FilePerScan)
                    {
                        for (int i = 0; i < scans.Count; i++)
                        {
                            var subPath = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now, true, i);
                            if (extension.ToLower() == ".pdf")
                            {
                                pdfExporter.Export(subPath, scans[i], pdfSettingsContainer.PdfSettings,
                                    userConfigManager.Config.OcrLanguageCode, j => true);
                            }
                            else
                            {
                                // TODO: Verify behavior for TIFF + others
                                imageSaver.SaveImages(subPath, now, scans[i], j => true);
                            }
                        }
                    }
                    else if (Settings.SaveSeparator == BatchSaveSeparator.FilePerPage)
                    {
                        for (int i = 0; i < allImages.Count; i++)
                        {
                            var subPath = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now, true, i);
                            if (extension.ToLower() == ".pdf")
                            {
                                pdfExporter.Export(subPath, new[] { allImages[i] }, pdfSettingsContainer.PdfSettings,
                                    userConfigManager.Config.OcrLanguageCode, j => true);
                            }
                            else
                            {
                                // TODO: Verify behavior for TIFF + others
                                imageSaver.SaveImages(subPath, now, new [] { allImages[i] }, j => true);
                            }
                        }
                    }
                    else if (Settings.SaveSeparator == BatchSaveSeparator.PatchT)
                    {
                        // TODO
                    }
                }
            }
        }
    }
}
