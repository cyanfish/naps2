using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private Bitmap placeholder;

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
                    ilThumbnailList.Images.Add(GetThumbnail(images[i]));
                    Debug.Assert(selection == null);
                }
                else
                {
                    ilThumbnailList.Images[i] = GetThumbnail(images[i]);
                }
            }

            Invalidate();
        }

        public void AppendImage(ScannedImage img)
        {
            ilThumbnailList.Images.Add(GetThumbnail(img));
            Items.Add(PlatformCompat.Runtime.UseSpaceInListViewItem ? " " : "", ilThumbnailList.Images.Count - 1);
        }

        public void SetDirty(int index, ScannedImage img)
        {
            ilThumbnailList.Images[index] = DrawHourglass(ilThumbnailList.Images[index]);
            Invalidate(Items[index].Bounds);
        }

        public void ReplaceThumbnail(int index, ScannedImage img)
        {
            ilThumbnailList.Images[index] = GetThumbnail(img);
            Invalidate(Items[index].Bounds);
        }

        public void RegenerateThumbnailList(List<ScannedImage> images)
        {
            if (ilThumbnailList.Images.Count > 0)
            {
                ilThumbnailList.Images.Clear();
            }
            var list = new List<Image>();
            foreach (var image in images)
            {
                list.Add(GetThumbnail(image));
            }
            ilThumbnailList.Images.AddRange(list.ToArray());
        }

        private Bitmap GetThumbnail(ScannedImage img)
        {
            return img.GetThumbnail() ?? RenderPlaceholder();
        }

        private Bitmap RenderPlaceholder()
        {
            lock (this)
            {
                if (placeholder?.Size == ThumbnailSize)
                {
                    return placeholder;
                }
                placeholder?.Dispose();
                placeholder = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);
                DrawHourglass(placeholder);
                return placeholder;
            }
        }

        private Image DrawHourglass(Image image)
        {
            using (var g = Graphics.FromImage(image))
            {
                g.DrawImage(Icons.hourglass_grey, new Rectangle((image.Width - 32) / 2, (image.Height - 32) / 2, 32, 32));
            }
            return image;
        }
    }
}
