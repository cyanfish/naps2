/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2015       Luca De Petrillo
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

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettings
    {
        private PdfMetadata metadata;
        private PdfImageSettings imageSettings;
        private PdfEncryption encryption;

        public PdfSettings()
        {
            metadata = new PdfMetadata();
            imageSettings = new PdfImageSettings();
            encryption = new PdfEncryption();
        }

        public string DefaultFileName { get; set; }

        public PdfMetadata Metadata
        {
            get { return metadata; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                metadata = value;
            }
        }

        public PdfImageSettings ImageSettings
        {
            get { return imageSettings; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                imageSettings = value;
            }
        }

        public PdfEncryption Encryption
        {
            get { return encryption; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                encryption = value;
            }
        }
    }
}
