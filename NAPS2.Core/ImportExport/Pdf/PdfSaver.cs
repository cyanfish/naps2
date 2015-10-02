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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSaver
    {
        private readonly IErrorOutput errorOutput;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly IPdfExporter pdfExporter;
        private readonly IOverwritePrompt overwritePrompt;

        public PdfSaver(IErrorOutput errorOutput, FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOverwritePrompt overwritePrompt)
        {
            this.errorOutput = errorOutput;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.overwritePrompt = overwritePrompt;
        }

        public bool SavePdf(string fileName, DateTime dateTime, ICollection<IScannedImage> images, PdfSettings pdfSettings, string ocrLanguageCode, Func<int, bool> progressCallback)
        {
            var subFileName = fileNamePlaceholders.SubstitutePlaceholders(fileName, dateTime);
            if (File.Exists(subFileName))
            {
                if (overwritePrompt.ConfirmOverwrite(subFileName) != DialogResult.Yes)
                {
                    return false;
                }
            }

            try
            {
                pdfExporter.Export(subFileName, images, pdfSettings, ocrLanguageCode, progressCallback);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorOutput.DisplayError(MiscResources.DontHavePermission);
            }
            catch (Exception ex)
            {
                Log.ErrorException(MiscResources.ErrorSaving, ex);
                errorOutput.DisplayError(MiscResources.ErrorSaving);
            }
            return false;
        }
    }
}
