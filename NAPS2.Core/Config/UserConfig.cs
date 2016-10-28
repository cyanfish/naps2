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
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Batch;
using NAPS2.Scan.Images;

namespace NAPS2.Config
{
    public class UserConfig
    {
        public UserConfig()
        {
            FormStates = new List<FormState>();
            CustomPageSizePresets = new List<NamedPageSize>();
            ThumbnailSize = ThumbnailRenderer.DEFAULT_SIZE;
        }

        public const int CURRENT_VERSION = 2;

        public int Version { get; set; }

        public string Culture { get; set; }

        public List<FormState> FormStates { get; set; }

        public DateTime? LastUpdateCheckDate { get; set; }

        public bool EnableOcr { get; set; }

        public string OcrLanguageCode { get; set; }

        public string LastImageExt { get; set; }

        public PdfSettings PdfSettings { get; set; }

        public ImageSettings ImageSettings { get; set; }

        public EmailSettings EmailSettings { get; set; }

        public int ThumbnailSize { get; set; }

        public BatchSettings LastBatchSettings { get; set; }

        public DockStyle DesktopToolStripDock { get; set; }

        public KeyboardShortcuts KeyboardShortcuts { get; set; }

        public List<NamedPageSize> CustomPageSizePresets { get; set; }
    }
}
