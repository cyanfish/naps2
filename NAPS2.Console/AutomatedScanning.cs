/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Console.Lang.Resources;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Console
{
    public class AutomatedScanning : IScanReceiver
    {
        private readonly ImageSaver imageSaver;
        private readonly IPdfExporter pdfExporter;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IErrorOutput errorOutput;

        private readonly AutomatedScanningOptions options;
        private List<IScannedImage> scannedImages;
        private int pagesScanned;

        public AutomatedScanning(AutomatedScanningOptions options, ImageSaver imageSaver, IPdfExporter pdfExporter, IProfileManager profileManager, IScanPerformer scanPerformer, IErrorOutput errorOutput)
        {
            this.options = options;
            this.imageSaver = imageSaver;
            this.pdfExporter = pdfExporter;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.errorOutput = errorOutput;
        }

        public void Execute()
        {
            ExtendedScanSettings profile;
            if (!GetProfile(out profile))
            {
                return;
            }

            PerformScan(profile);

            ExportScannedImages();

            if (options.WaitForEnter)
            {
                System.Console.ReadLine();
            }
        }

        private void ExportScannedImages()
        {
            if (scannedImages.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToExport);
                return;
            }

            if (options.Verbose)
            {
                System.Console.WriteLine(ConsoleResources.Exporting);
            }

            string extension = Path.GetExtension(options.OutputPath);
            Debug.Assert(extension != null);
            if (extension.ToLower() == ".pdf")
            {
                ExportToPdf();
            }
            else
            {
                ExportToImageFiles();
            }
        }

        private void ExportToImageFiles()
        {
            imageSaver.SaveImages(options.OutputPath, scannedImages, path =>
            {
                NotifyOverwrite(path);
                return options.ForceOverwrite;
            });

            if (options.Verbose)
            {
                System.Console.WriteLine(ConsoleResources.FinishedSavingImages, options.OutputPath);
            }
        }

        private void NotifyOverwrite(string path)
        {
            if (!options.ForceOverwrite)
            {
                errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
            }
            if (options.ForceOverwrite && options.Verbose)
            {
                System.Console.WriteLine(ConsoleResources.Overwriting, path);
            }
        }

        private void ExportToPdf()
        {
            if (File.Exists(options.OutputPath))
            {
                NotifyOverwrite(options.OutputPath);
                if (!options.ForceOverwrite)
                {
                    return;
                }
            }
            var pdfInfo = new PdfInfo
            {
                Title = ConsoleResources.ScannedImage,
                Subject = ConsoleResources.ScannedImage,
                Author = ConsoleResources.NAPS2
            };

            try
            {
                pdfExporter.Export(options.OutputPath, scannedImages.Select(x => (Image)x.GetImage()), pdfInfo, i =>
                {
                    if (options.Verbose)
                    {
                        System.Console.WriteLine(ConsoleResources.ExportedPage, i, scannedImages.Count);
                    }
                    return true;
                });

                if (options.Verbose)
                {
                    System.Console.WriteLine(ConsoleResources.SuccessfullySavedPdf, options.OutputPath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                errorOutput.DisplayError(ConsoleResources.DontHavePermission);
            }
        }

        private void PerformScan(ExtendedScanSettings profile)
        {
            if (options.Verbose)
            {
                System.Console.WriteLine(ConsoleResources.BeginningScan);
            }

            scannedImages = new List<IScannedImage>();
            IWin32Window parentWindow = new Form { Visible = false };
            foreach (int i in Enumerable.Range(1, options.Number))
            {
                if (options.Delay > 0)
                {
                    if (options.Verbose)
                    {
                        System.Console.WriteLine(ConsoleResources.Waiting, options.Delay);
                    }
                    Thread.Sleep(options.Delay);
                }
                if (options.Verbose)
                {
                    System.Console.WriteLine(ConsoleResources.StartingScan, i, options.Number);
                }
                pagesScanned = 0;
                scanPerformer.PerformScan(profile, parentWindow, this);
                if (options.Verbose)
                {
                    System.Console.WriteLine(ConsoleResources.PagesScanned, pagesScanned);
                }
            }
        }

        private bool GetProfile(out ExtendedScanSettings profile)
        {
            try
            {
                if (options.ProfileName == null)
                {
                    // If no profile is specified, use the default (if there is one)
                    profile = profileManager.Profiles.Single(x => x.IsDefault);
                }
                else
                {
                    // Use the profile with the specified name (case-sensitive)
                    profile = profileManager.Profiles.FirstOrDefault(x => x.DisplayName == options.ProfileName);
                    if (profile == null)
                    {
                        // If none found, try case-insensitive
                        profile = profileManager.Profiles.First(x => x.DisplayName.ToLower() == options.ProfileName.ToLower());
                    }
                }
            }
            catch (InvalidOperationException)
            {
                errorOutput.DisplayError(ConsoleResources.ProfileUnavailableOrAmbiguous);
                profile = null;
                return false;
            }
            return true;
        }

        public void ReceiveScannedImage(IScannedImage scannedImage)
        {
            scannedImages.Add(scannedImage);
            pagesScanned++;
        }
    }
}
