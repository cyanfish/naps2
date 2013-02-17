/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009        Pavel Sorejs
    Copyright (C) 2012, 2013  Ben Olden-Cooligan

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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NAPS
{
    public partial class ThumbnailList : ListView
    {
        public ThumbnailList()
        {
            InitializeComponent();
            this.LargeImageList = ilThumbnailList;
        }

        public void UpdateImages(SortedList<int,CScannedImage> images)
        {
            ilThumbnailList.Images.Clear();
            this.Clear();
            foreach (int id in images.Keys)
            {
                ilThumbnailList.Images.Add(images[id].Thumbnail);
                ListViewItem item = this.Items.Add("", ilThumbnailList.Images.Count - 1);
                item.Tag = id;
            }
        }

        public void UpdateView(SortedList<int, CScannedImage> images)
        {
            ilThumbnailList.Images.Clear();
            foreach (int id in images.Keys)
            {
                ilThumbnailList.Images.Add(images[id].Thumbnail);
            }
        }

        public void ClearItems()
        {
            this.Clear();
            ilThumbnailList.Images.Clear();
        }
    }
}
