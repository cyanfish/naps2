using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Platform;
using NAPS2.Scan.Images;

namespace NAPS2.WinForms
{
    public partial class ThumbnailList : DragScrollListView
    {
        private static readonly FieldInfo imageSizeField;
        private static readonly MethodInfo performRecreateHandleMethod;

        static ThumbnailList()
        {
            if (PlatformCompat.Runtime.SetImageListSizeOnImageCollection)
            {
                imageSizeField = typeof(ImageList.ImageCollection).GetField("imageSize", BindingFlags.Instance | BindingFlags.NonPublic);
                performRecreateHandleMethod = typeof(ImageList.ImageCollection).GetMethod("RecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            else
            {
                imageSizeField = typeof(ImageList).GetField("imageSize", BindingFlags.Instance | BindingFlags.NonPublic);
                performRecreateHandleMethod = typeof(ImageList).GetMethod("PerformRecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (imageSizeField == null || performRecreateHandleMethod == null)
            {
                // No joy, just be happy enough with 256
                ThumbnailRenderer.MAX_SIZE = 256;
            }
        }

        public ThumbnailList()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            LargeImageList = ilThumbnailList;
        }

        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        public Size ThumbnailSize
        {
            get => ilThumbnailList.ImageSize;
            set
            {
                if (imageSizeField != null && performRecreateHandleMethod != null)
                {
                    // A simple hack to let the listview have larger thumbnails than 256x256
                    if (PlatformCompat.Runtime.SetImageListSizeOnImageCollection)
                    {
                        imageSizeField.SetValue(ilThumbnailList.Images, value);
                        performRecreateHandleMethod.Invoke(ilThumbnailList.Images, new object[] { });
                    }
                    else
                    {
                        imageSizeField.SetValue(ilThumbnailList, value);
                        performRecreateHandleMethod.Invoke(ilThumbnailList, new object[] { "ImageSize" });
                    }
                }
                else
                {
                    ilThumbnailList.ImageSize = value;
                }
            }
        }

        public void UpdateImages(List<ScannedImage> images, List<int> selection = null)
        {
            if (images.Count == 0)
            {
                // Fast case
                Items.Clear();
                ilThumbnailList.Images.Clear();
            }
            else
            {
                int delta = images.Count - Items.Count;
                for (int i = 0; i < delta; i++)
                {
                    Items.Add("", i);
                    Debug.Assert(selection == null);
                }
                for (int i = 0; i < -delta; i++)
                {
                    Items.RemoveAt(Items.Count - 1);
                    ilThumbnailList.Images.RemoveAt(ilThumbnailList.Images.Count - 1);
                    Debug.Assert(selection == null);
                }
            }

            // Determine the smallest range that contains all images in the selection
            int min = selection == null || !selection.Any() ? 0 : selection.Min();
            int max = selection == null || !selection.Any() ? images.Count : selection.Max() + 1;

            for (int i = min; i < max; i++)
            {
                if (i >= ilThumbnailList.Images.Count)
                {
                    ilThumbnailList.Images.Add(images[i].GetThumbnail(ThumbnailRenderer));
                    Debug.Assert(selection == null);
                }
                else
                {
                    ilThumbnailList.Images[i] = images[i].GetThumbnail(ThumbnailRenderer);
                }
            }

            Invalidate();
        }

        public void AppendImage(ScannedImage img)
        {
            ilThumbnailList.Images.Add(img.GetThumbnail(ThumbnailRenderer));
            Items.Add("", ilThumbnailList.Images.Count - 1);
        }

        public void ReplaceThumbnail(int index, ScannedImage img)
        {
            ilThumbnailList.Images[index] = img.GetThumbnail(ThumbnailRenderer);
            Invalidate(Items[index].Bounds);
        }

        public void RegenerateThumbnailList(List<ScannedImage> images)
        {
            if (ilThumbnailList.Images.Count > 0)
            {
                ilThumbnailList.Images.Clear();
            }
            var thumbnailArray = images.Select(x => (Image)x.GetThumbnail(ThumbnailRenderer)).ToArray();
            ilThumbnailList.Images.AddRange(thumbnailArray);
        }
    }
}
