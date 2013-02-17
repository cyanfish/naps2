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