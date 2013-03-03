/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
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
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;

namespace NAPS2
{
    public partial class ThumbnailList : ListView
    {
        public ThumbnailList()
        {
            InitializeComponent();
            LargeImageList = ilThumbnailList;
        }

        public void UpdateImages(List<IScannedImage> images)
        {
            ilThumbnailList.Images.Clear();
            Clear();
            foreach (IScannedImage img in images)
            {
                ilThumbnailList.Images.Add(img.Thumbnail);
                ListViewItem item = Items.Add("", ilThumbnailList.Images.Count - 1);
            }
        }

        public void UpdateView(List<IScannedImage> images)
        {
            ilThumbnailList.Images.Clear();
            foreach (IScannedImage img in images)
            {
                ilThumbnailList.Images.Add(img.Thumbnail);
            }
        }

        public void ClearItems()
        {
            Clear();
            ilThumbnailList.Images.Clear();
        }
    }
}
