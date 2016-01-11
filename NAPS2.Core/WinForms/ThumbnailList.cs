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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Scan.Images;

namespace NAPS2.WinForms
{
    public partial class ThumbnailList : ListView
    {
        private ThumbnailCache thumbnails;

        public ThumbnailList()
        {
            InitializeComponent();
            LargeImageList = ilThumbnailList;
        }

        public IUserConfigManager UserConfigManager
        {
            set
            {
                thumbnails = new ThumbnailCache(value);
            }
        }

        public Size ThumbnailSize
        {
            get { return ilThumbnailList.ImageSize; }
            set { ilThumbnailList.ImageSize = value; }
        }

        public void UpdateImages(List<ScannedImage> images)
        {
            ilThumbnailList.Images.Clear();
            Clear();
            foreach (ScannedImage img in images)
            {
                AppendImage(img);
            }
            thumbnails.TrimCache(images);
        }

        public void AppendImage(ScannedImage img)
        {
            ilThumbnailList.Images.Add(thumbnails[img]);
            Items.Add("", ilThumbnailList.Images.Count - 1);
        }

        public void ReplaceThumbnail(int index, ScannedImage img)
        {
            ilThumbnailList.Images[index] = thumbnails[img];
            Invalidate(Items[index].Bounds);
        }

        public void RegenerateThumbnailList(List<ScannedImage> images)
        {
            var thumbnailArray = images.Select(x => (Image)thumbnails[x]).ToArray();
            ilThumbnailList.Images.AddRange(thumbnailArray);
        }
    }
}
