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

        public void PerformBatchScan(BatchSettings settings, IWin32Window dialogParent, IScanReceiver scanReceiver, Func<string, bool> progressCallback)
        {
            var state = new BatchState(scanPerformer, profileManager, fileNamePlaceholders, pdfExporter, imageSaver, pdfSettingsContainer, userConfigManager)
            {
                Settings = settings,
                ProgressCallback = progressCallback,
                DialogParent = dialogParent,
                LoadImagesScanReceiver = scanReceiver
            };
            state.Do();
        }

        private class BatchState : IScanReceiver
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

            public IScanReceiver LoadImagesScanReceiver { get; set; }

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
                scans.Add(new List<IScannedImage>());
                int pageNumber = 1;
                scanPerformer.PerformScan(profile, DialogParent, this, () =>
                {
                    if (scanNumber == -1)
                    {
                        ProgressCallback(string.Format(MiscResources.BatchStatusPage, pageNumber + 1));
                    }
                    else
                    {
                        ProgressCallback(string.Format(MiscResources.BatchStatusScanPage, scanNumber + 1, pageNumber + 1));
                    }
                    pageNumber++;
                });
            }
            
            public void ReceiveScannedImage(IScannedImage scannedImage)
            {
                scans.Last().Add(scannedImage);
            }

            private bool PromptForNextScan()
            {
                throw new NotImplementedException();
            }

            private void Output()
            {
                var now = DateTime.Now;
                var allImages = scans.SelectMany(x => x);

                if (Settings.OutputType == BatchOutputType.Load)
                {
                    foreach (var image in allImages)
                    {
                        LoadImagesScanReceiver.ReceiveScannedImage(image);
                    }
                }
                else if (Settings.OutputType == BatchOutputType.SingleFile)
                {
                    var path = fileNamePlaceholders.SubstitutePlaceholders(Settings.SavePath, now);
                    string extension = Path.GetExtension(path);
                    Debug.Assert(extension != null);
                    if (extension.ToLower() == ".pdf")
                    {
                        pdfExporter.Export(path, allImages, pdfSettingsContainer.PdfSettings,
                            userConfigManager.Config.OcrLanguageCode, i => true);
                    }
                    else
                    {
                        
                    }
                }
            }
        }
    }
}
