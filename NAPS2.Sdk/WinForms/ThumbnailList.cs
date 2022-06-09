using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NAPS2.EtoForms.WinForms;
using NAPS2.Images.Gdi;

namespace NAPS2.WinForms
{
    public partial class ThumbnailList : DragScrollListView
    {
        private Bitmap _placeholder;

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
            set => ListViewImageSizeHack.SetImageSize(ilThumbnailList, value);
        }

        private string ItemText => PlatformCompat.Runtime.UseSpaceInListViewItem ? " " : "";

        private List<ImageInfo> CurrentImages => Items.Cast<ListViewItem>().Select(x => (ImageInfo)x.Tag).ToList();

        private ImageInfo GetImageInfo(int i) => (ImageInfo) Items[i].Tag;

        public void UpdatedImages(List<ProcessedImage> images, out bool orderingChanged)
        {
            lock (this)
            {
                orderingChanged = false;
                BeginUpdate();

                if (images.Count == 0)
                {
                    ilThumbnailList.Images.Clear();
                    Items.Clear();
                }
                else
                {
                    DeleteExcessImages(images);
                    AddMissingImages(images);
                    UpdateChangedImages(images, ref orderingChanged);
                }
                EndUpdate();
            }
            Invalidate();
        }

        private void UpdateChangedImages(List<ProcessedImage> images, ref bool orderingChanged)
        {
            for (int i = 0; i < ilThumbnailList.Images.Count; i++)
            {
                var imageInfo = GetImageInfo(i);
                if (imageInfo.Image != images[i])
                {
                    orderingChanged = true;
                }
                if (imageInfo.Image != images[i] || imageInfo.TransformState != images[i].TransformState)
                {
                    ilThumbnailList.Images[i] = GetThumbnail(images[i]);
                    Items[i].Tag = new ImageInfo(images[i]);
                }
            }
        }

        private void DeleteExcessImages(List<ProcessedImage> images)
        {
            foreach (var oldImg in CurrentImages.Select(x => x.Image).Except(images))
            {
                var item = Items.Cast<ListViewItem>().First(x => ((ImageInfo)x.Tag).Image == oldImg);
                foreach (ListViewItem item2 in Items)
                {
                    if (item2.ImageIndex > item.ImageIndex)
                    {
                        item2.ImageIndex -= 1;
                    }
                }

                ilThumbnailList.Images.RemoveAt(item.ImageIndex);
                Items.RemoveAt(item.Index);
            }
        }

        private void AddMissingImages(List<ProcessedImage> images)
        {
            for (int i = ilThumbnailList.Images.Count; i < images.Count; i++)
            {
                ilThumbnailList.Images.Add(GetThumbnail(images[i]));
                Items.Add(ItemText, i).Tag = new ImageInfo(images[i]);
            }
        }

        public void ReplaceThumbnail(int index, ProcessedImage img)
        {
            lock (this)
            {
                BeginUpdate();
                var thumb = GetThumbnail(img);
                if (thumb.Size == ThumbnailSize)
                {
                    ilThumbnailList.Images[index] = thumb;
                    Invalidate(Items[index].Bounds);
                }
                EndUpdate();
            }
        }

        public void RegenerateThumbnailList(List<ProcessedImage> images)
        {
            lock (this)
            {
                BeginUpdate();
                if (ilThumbnailList.Images.Count > 0)
                {
                    ilThumbnailList.Images.Clear();
                }

                var list = new List<Image>();
                foreach (var image in images)
                {
                    list.Add(GetThumbnail(image));
                }

                foreach (ListViewItem item in Items)
                {
                    item.ImageIndex = item.Index;
                }

                ilThumbnailList.Images.AddRange(list.ToArray());
                EndUpdate();
            }
        }

        private Bitmap GetThumbnail(ProcessedImage img)
        {
            lock (this)
            {
                // TODO: UiImage
                return null;
                // var thumb = ((GdiImage)img.GetThumbnail())?.Bitmap;
                // if (thumb == null)
                // {
                //     return RenderPlaceholder();
                // }
                // if (img.IsThumbnailDirty)
                // {
                //     thumb = DrawHourglass(thumb);
                // }
                // return thumb;
            }
        }

        private Bitmap RenderPlaceholder()
        {
            lock (this)
            {
                if (_placeholder?.Size == ThumbnailSize)
                {
                    return _placeholder;
                }
                _placeholder?.Dispose();
                _placeholder = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);
                _placeholder = DrawHourglass(_placeholder);
                return _placeholder;
            }
        }

        private Bitmap DrawHourglass(Image image)
        {
            var bitmap = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(new ColorMatrix
                {
                    Matrix33 = 0.3f
                });
                g.DrawImage(image,
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    attrs);
                g.DrawImage(Icons.hourglass_grey, new Rectangle((bitmap.Width - 32) / 2, (bitmap.Height - 32) / 2, 32, 32));
            }
            image.Dispose();
            return bitmap;
        }

        private class ImageInfo
        {
            public ImageInfo(ProcessedImage image)
            {
                Image = image;
                TransformState = image.TransformState;
            }

            public ProcessedImage Image { get; set; }
            
            public TransformState TransformState { get; set; }
        }
    }
}
