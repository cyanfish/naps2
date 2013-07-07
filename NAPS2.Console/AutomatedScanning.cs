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

        private readonly AutomatedScanningOptions options;
        private List<IScannedImage> scannedImages;
        private int pagesScanned;

        public AutomatedScanning(AutomatedScanningOptions options, ImageSaver imageSaver, IPdfExporter pdfExporter, IProfileManager profileManager, IScanPerformer scanPerformer)
        {
            this.options = options;
            this.imageSaver = imageSaver;
            this.pdfExporter = pdfExporter;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
        }

        public void Execute()
        {
            ScanSettings profile;
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
                System.Console.WriteLine("No scanned pages to export.");
                return;
            }

            if (options.Verbose)
            {
                System.Console.WriteLine("Exporting...");
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
                System.Console.WriteLine("Finished saving images to {0}", options.OutputPath);
            }
        }

        private void NotifyOverwrite(string path)
        {
            if (!options.ForceOverwrite)
            {
                System.Console.WriteLine("File already exists. Use --force to overwrite. Path: {0}", path);
            }
            if (options.ForceOverwrite && options.Verbose)
            {
                System.Console.WriteLine("Overwriting: {0}", path);
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
                Title = "Scanned Image",
                Subject = "Scanned Image",
                Author = "NAPS2"
            };
            pdfExporter.Export(options.OutputPath, scannedImages.Select(x => (Image) x.GetImage()), pdfInfo, i =>
            {
                if (options.Verbose)
                {
                    System.Console.WriteLine("Exported page {0} of {1}.", i, scannedImages.Count);
                }
                return true;
            });

            if (options.Verbose)
            {
                System.Console.WriteLine("Successfully saved PDF file to {0}", options.OutputPath);
            }
        }

        private void PerformScan(ScanSettings profile)
        {
            if (options.Verbose)
            {
                System.Console.WriteLine("Beginning scan...");
            }

            scannedImages = new List<IScannedImage>();
            IWin32Window parentWindow = new Form {Visible = false};
            foreach (int i in Enumerable.Range(1, options.Number))
            {
                if (options.Delay > 0)
                {
                    if (options.Verbose)
                    {
                        System.Console.WriteLine("Waiting {0}ms...", options.Delay);
                    }
                    Thread.Sleep(options.Delay);
                }
                if (options.Verbose)
                {
                    System.Console.WriteLine("Starting scan {0} of {1}...", i, options.Number);
                }
                pagesScanned = 0;
                scanPerformer.PerformScan(profile, parentWindow, this);
                if (options.Verbose)
                {
                    System.Console.WriteLine("{0} page(s) scanned.", pagesScanned);
                }
            }
        }

        private bool GetProfile(out ScanSettings profile)
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
                System.Console.WriteLine("The specified profile is unavailable or ambiguous.");
                System.Console.WriteLine("Use the --profile option to specify a profile by name.");
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
