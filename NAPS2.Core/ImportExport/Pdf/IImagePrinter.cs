/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

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
using System.Drawing.Printing;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Pdf
{
    public interface IImagePrinter
    {
        /// <summary>
        /// Prints the provided images, prompting the user for the printer settings.
        /// </summary>
        /// <param name="images">The full list of images to print.</param>
        /// <param name="selectedImages">The list of selected images. If non-empty, the user will be presented an option to print selected.</param>
        /// <returns>True if the print completed, false if there was nothing to print or the user cancelled.</returns>
        bool PromptToPrint(List<IScannedImage> images, List<IScannedImage> selectedImages);

        /// <summary>
        /// Prints the provided images with the specified printer settings.
        /// </summary>
        /// <param name="printerSettings">The printer settings.</param>
        /// <param name="images">The full list of images to print.</param>
        /// <param name="selectedImages">The list of selected images, to be used if the printer settings specify to print selected.</param>
        /// <returns>True if the print completed, false if there was nothing to print.</returns>
        bool Print(PrinterSettings printerSettings, List<IScannedImage> images, List<IScannedImage> selectedImages);
    }
}
