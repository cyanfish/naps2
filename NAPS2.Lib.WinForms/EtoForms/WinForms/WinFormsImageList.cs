using System.Drawing;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.WinForms;

public abstract class WinFormsImageList<T> where T : notnull
{
    private readonly WinFormsListView<T> _listView;
    private readonly ListViewBehavior<T> _behavior;

    private WinFormsImageList(WinFormsListView<T> listView, ListViewBehavior<T> behavior)
    {
        _listView = listView;
        _behavior = behavior;
    }

    private Image ItemToImage(T item)
    {
        return _behavior.GetImage(_listView, item).ToSD();
    }

    public abstract void Clear();
    public abstract void Append(T item, ListViewItem listViewItem);
    public abstract void Replace(T item, int index);
    public abstract void DeleteFromEnd();
    public abstract Image Get(ListViewItem listViewItem);
    public abstract Image PartialAppend(T item);
    public abstract void FinishPartialAppends(List<Image> images);

    public class Custom : WinFormsImageList<T>
    {
        private readonly List<Image> _images = [];

        public Custom(WinFormsListView<T> listView, ListViewBehavior<T> behavior) : base(listView, behavior)
        {
        }

        public override void Clear()
        {
            foreach (var image in _images)
            {
                image.Dispose();
            }
            _images.Clear();
        }

        public override void Append(T item, ListViewItem listViewItem)
        {
            _images.Add(ItemToImage(item));
        }

        public override void Replace(T item, int index)
        {
            _images[index].Dispose();
            _images[index] = ItemToImage(item);
        }

        public override void DeleteFromEnd()
        {
            _images[_images.Count - 1].Dispose();
            _images.RemoveAt(_images.Count - 1);
        }

        public override Image Get(ListViewItem listViewItem)
        {
            return _images[listViewItem.Index];
        }

        public override Image PartialAppend(T item)
        {
            return ItemToImage(item);
        }

        public override void FinishPartialAppends(List<Image> images)
        {
            _images.AddRange(images);
        }
    }

    public class Native : WinFormsImageList<T>
    {
        private readonly ImageList.ImageCollection _images;

        public Native(WinFormsListView<T> listView, ListViewBehavior<T> behavior) : base(listView, behavior)
        {
            _images = _listView.NativeControl.LargeImageList!.Images;
        }

        public override void Clear()
        {
            _images.Clear();
        }

        public override void Append(T item, ListViewItem listViewItem)
        {
            _images.Add(_behavior.GetImage(_listView, item).ToSD());
            listViewItem.ImageIndex = _images.Count - 1;
        }

        public override void Replace(T item, int index)
        {
            _images[index] = ItemToImage(item);
        }

        public override void DeleteFromEnd()
        {
            _images.RemoveAt(_images.Count - 1);
        }

        public override Image Get(ListViewItem listViewItem) => throw new NotSupportedException();

        public override Image PartialAppend(T item)
        {
            return ItemToImage(item);
        }

        public override void FinishPartialAppends(List<Image> images)
        {
            _images.AddRange(images.ToArray());
        }
    }

    public class Stub : WinFormsImageList<T>
    {
        public Stub(WinFormsListView<T> listView, ListViewBehavior<T> behavior) : base(listView, behavior)
        {
        }

        public override void Clear()
        {
        }

        public override void Append(T item, ListViewItem listViewItem)
        {
        }

        public override void Replace(T item, int index)
        {
        }

        public override void DeleteFromEnd()
        {
        }

        public override Image Get(ListViewItem listViewItem) => throw new NotSupportedException();

        public override Image PartialAppend(T item)
        {
            return null!;
        }

        public override void FinishPartialAppends(List<Image> images)
        {
        }
    }
}