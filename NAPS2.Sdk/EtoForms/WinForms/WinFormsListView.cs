using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.Platform;
using NAPS2.Util;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsListView<T> : IListView<T>
{
    private readonly ListView _view;
    private readonly ListViewBehavior<T> _behavior;

    private ListSelection<T> _selection = ListSelection.Empty<T>();
    private bool _refreshing;

    public WinFormsListView(ListViewBehavior<T> behavior)
    {
        _behavior = behavior;
        _view = new ListView
        {
            LargeImageList = new ImageList(),
            View = View.LargeIcon,
            MultiSelect = behavior.MultiSelect
        };

        _view.SelectedIndexChanged += OnSelectedIndexChanged;
        _view.ItemActivate += OnItemActivate;
        _view.ItemDrag += OnItemDrag;
        _view.DragEnter += OnDragEnter;
        _view.DragDrop += OnDragDrop;
        _view.DragOver += OnDragOver;
        _view.DragLeave += OnDragLeave;
    }
        
    public Eto.Drawing.Size ImageSize
    {
        get => _view.LargeImageList.ImageSize.ToEto();
        set => ListViewImageSizeHack.SetImageSize(_view.LargeImageList, value.ToSD());
    }

    public bool AllowDrag { get; set; }
        
    public bool AllowDrop
    {
        get => _view.AllowDrop;
        set => _view.AllowDrop = value;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        if (!AllowDrop)
        {
            return;
        }
        e.Effect = _behavior.GetDropEffect(e.Data.ToEto()).ToSwf();
    }

    public Eto.Forms.Control Control => Eto.Forms.WinFormsHelpers.ToEto(_view);

    public event EventHandler? SelectionChanged;
        
    public event EventHandler? ItemClicked;

    public event EventHandler<DropEventArgs>? Drop;

    public void SetItems(IEnumerable<T> items)
    {
        _refreshing = true;
        _view.Items.Clear();
        _view.LargeImageList.Images.Clear();
        foreach (var item in items)
        {
            _view.LargeImageList.Images.Add(_behavior.GetImage(item).ToSD());
            var listViewItem = _view.Items.Add(GetLabel(item), _view.LargeImageList.Images.Count - 1);
            listViewItem.Tag = item;
        }
        for (int i = 0; i < _view.Items.Count; i++)
        {
            _view.Items[i].Selected = Selection.Contains((T) _view.Items[i].Tag);
        }
        _refreshing = false;
    }

    public ListSelection<T> Selection
    {
        get => _selection;
        set
        {
            _selection = value ?? throw new ArgumentNullException(nameof(value));
            if (!_refreshing)
            {
                _refreshing = true;
                for (int i = 0; i < _view.Items.Count; i++)
                {
                    _view.Items[i].Selected = _selection.Contains((T) _view.Items[i].Tag);
                }
                _refreshing = false;
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private string GetLabel(T item)
    {
        if (!_behavior.ShowLabels)
        {
            return PlatformCompat.Runtime.UseSpaceInListViewItem ? " " : "";
        }
        return _behavior.GetLabel(item);
    }

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_refreshing)
        {
            _refreshing = true;
            Selection = ListSelection.From(_view.SelectedItems.Cast<ListViewItem>().Select(x => (T) x.Tag));
            _refreshing = false;
        }
    }
        
    private void OnItemActivate(object sender, EventArgs e)
    {
        ItemClicked?.Invoke(this, EventArgs.Empty);
    }
        
    private void OnItemDrag(object sender, ItemDragEventArgs e)
    {
        if (!AllowDrag)
        {
            return;
        }
        // Provide drag data
        if (Selection.Count > 0)
        {
            var dataObject = new DataObject();
            _behavior.SetDragData(Selection, dataObject.ToEto());
            _view.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy);
        }
    }
        
    private void OnDragDrop(object sender, DragEventArgs e)
    {
        var index = GetDragIndex(e);
        if (index != -1)
        {
            Drop?.Invoke(this, new DropEventArgs(index, e.Data));
        }
        _view.InsertionMark.Index = -1;
    }
        
    private void OnDragLeave(object sender, EventArgs e)
    {
        _view.InsertionMark.Index = -1;
    }
        
    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Effect == DragDropEffects.Move)
        {
            var index = GetDragIndex(e);
            if (index == _view.Items.Count)
            {
                _view.InsertionMark.Index = index - 1;
                _view.InsertionMark.AppearsAfterItem = true;
            }
            else
            {
                _view.InsertionMark.Index = index;
                _view.InsertionMark.AppearsAfterItem = false;
            }
        }
    }
        
    private int GetDragIndex(DragEventArgs e)
    {
        System.Drawing.Point cp = _view.PointToClient(new System.Drawing.Point(e.X, e.Y));
        ListViewItem? dragToItem = _view.GetItemAt(cp.X, cp.Y);
        if (dragToItem == null)
        {
            var items = _view.Items.Cast<ListViewItem>().ToList();
            var minY = items.Select(x => x.Bounds.Top).Min();
            var maxY = items.Select(x => x.Bounds.Bottom).Max();
            if (cp.Y < minY)
            {
                cp.Y = minY;
            }
            if (cp.Y > maxY)
            {
                cp.Y = maxY;
            }
            var row = items.Where(x => x.Bounds.Top <= cp.Y && x.Bounds.Bottom >= cp.Y).OrderBy(x => x.Bounds.X).ToList();
            dragToItem = row.FirstOrDefault(x => x.Bounds.Right >= cp.X) ?? row.LastOrDefault();
        }
        if (dragToItem == null)
        {
            return -1;
        }
        int dragToIndex = dragToItem.Index;
        if (cp.X > (dragToItem.Bounds.X + dragToItem.Bounds.Width / 2))
        {
            dragToIndex++;
        }
        return dragToIndex;
    }
}