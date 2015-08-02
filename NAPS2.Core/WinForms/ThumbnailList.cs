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
        public ThumbnailList()
        {
            InitializeComponent();
            LargeImageList = ilThumbnailList;
        }

        public IUserConfigManager UserConfigManager { get; set; }

        public Size ThumbnailSize
        {
            get { return ilThumbnailList.ImageSize; }
            set { ilThumbnailList.ImageSize = value; }
        }

        public void UpdateImages(List<IScannedImage> images)
        {
            ClearImages();
            Clear();
            foreach (IScannedImage img in images)
            {
                AppendImage(img);
            }
        }

        public void AppendImage(IScannedImage img)
        {
            ilThumbnailList.Images.Add(img.GetThumbnail(UserConfigManager.Config.ThumbnailSize));
            Items.Add("", ilThumbnailList.Images.Count - 1);
        }

        public void ClearItems()
        {
            ClearImages();
            Clear();
        }

        private void ClearImages()
        {
            foreach (Image img in ilThumbnailList.Images)
            {
                img.Dispose();
            }
            ilThumbnailList.Images.Clear();
        }

        public void ReplaceThumbnail(int index, Bitmap thumbnail)
        {
            ilThumbnailList.Images[index].Dispose();
            ilThumbnailList.Images[index] = thumbnail;
            Invalidate(Items[index].Bounds);
        }

        public void ReplaceThumbnailList(List<IScannedImage> images)
        {
            var thumbnails = images.Select(x => (Image)x.GetThumbnail(UserConfigManager.Config.ThumbnailSize)).ToArray();
            ilThumbnailList.Images.AddRange(thumbnails);
        }
    }
}
