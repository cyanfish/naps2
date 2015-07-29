/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

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
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Images;

namespace NAPS2.WinForms
{
    public partial class FPDFSave : FormBase
    {
        private readonly IPdfExporter pdfExporter;
        private readonly IErrorOutput errorOutput;
        private readonly IUserConfigManager userConfigManager;

        public FPDFSave(IPdfExporter pdfExporter, IErrorOutput errorOutput, IUserConfigManager userConfigManager)
        {
            InitializeComponent();
            this.pdfExporter = pdfExporter;
            this.errorOutput = errorOutput;
            this.userConfigManager = userConfigManager;
            RestoreFormState = false;
            Shown += FPDFSave_Shown;
        }

        public string Filename { get; set; }

        public IList<IScannedImage> Images { get; set; }

        private void ExportPdfProcess()
        {
            var info = new PdfInfo
            {
                Title = MiscResources.ScannedImage,
                Subject = MiscResources.ScannedImage,
                Author = MiscResources.NAPS2,
                Creator = MiscResources.NAPS2
            };
            var ocrLanguageCode = userConfigManager.Config.EnableOcr ? userConfigManager.Config.OcrLanguageCode : null;
            try
            {
                pdfExporter.Export(Filename, Images, info, ocrLanguageCode, num =>
                {
                    Invoke(new ThreadStart(() => SetStatus(num, Images.Count)));
                    return true;
                });
            }
            catch (UnauthorizedAccessException)
            {
                errorOutput.DisplayError(MiscResources.DontHavePermission);
            }

            Invoke(new ThreadStart(Close));
        }

        void FPDFSave_Shown(object sender, EventArgs e)
        {
            new Thread(ExportPdfProcess).Start();
        }

        public void SetStatus(int count, int total)
        {
            lblStatus.Text = string.Format(MiscResources.PdfStatus, count.ToString("G"), total.ToString("G"));
            Application.DoEvents();
        }
    }

}
