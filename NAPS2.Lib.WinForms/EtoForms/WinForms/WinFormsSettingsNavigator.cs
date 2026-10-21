using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Eto.WinForms;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsSettingsNavigator : ISettingsNavigator
{
    private const int ITEM_HEIGHT = 36;
    // Space around each item's highlight, which leaves room for the focus outline
    private const int ITEM_MARGIN = 2;
    private const int ITEM_CORNER_RADIUS = 4;
    private const int ICON_SIZE = 16;
    private const int CARD_PADDING = 4;
    private const int CARD_CORNER_RADIUS = 6;
    private const ControlStyles DOUBLE_BUFFERED_STYLES = ControlStyles.UserPaint |
        ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw;

    private readonly Panel _card;
    private readonly NavigatorListBox _listBox;
    private readonly Dictionary<string, Image?> _icons = new();
    private int _hoverIndex = -1;
    private float _scale = 1f;

    public WinFormsSettingsNavigator()
    {
        _listBox = new NavigatorListBox
        {
            DrawMode = DrawMode.OwnerDrawFixed,
            BorderStyle = BorderStyle.None,
            IntegralHeight = false,
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = SystemColors.Window,
            // Used for accessibility
            DisplayMember = nameof(SettingsNavigatorItem.Label)
        };
        // Do all the painting ourselves (double-buffered) to avoid flicker
        WinFormsHacks.SetControlStyle(_listBox, DOUBLE_BUFFERED_STYLES, true);
        _listBox.Paint += (_, e) => PaintItems(e);
        _listBox.SelectedIndexChanged += (_, _) =>
        {
            _listBox.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        };
        _listBox.MouseMove += (_, e) => SetHoverIndex(_listBox.IndexFromPoint(e.Location));
        _listBox.MouseLeave += (_, _) => SetHoverIndex(-1);
        _listBox.GotFocus += (_, _) => _listBox.Invalidate();
        _listBox.LostFocus += (_, _) => _listBox.Invalidate();
        _listBox.HandleCreated += (_, _) => UpdateScale();
        _listBox.DpiChangedAfterParent += (_, _) => UpdateScale();

        _card = new Panel { BackColor = SystemColors.Control };
        WinFormsHacks.SetControlStyle(_card, DOUBLE_BUFFERED_STYLES, true);
        _card.Paint += (_, e) => PaintCard(e);
        _card.Controls.Add(_listBox);
        Control = Eto.Forms.WinFormsHelpers.ToEto(_card);
    }

    public Eto.Forms.Control Control { get; }

    public int SelectedIndex
    {
        get => _listBox.SelectedIndex;
        set => _listBox.SelectedIndex = value;
    }

    public event EventHandler? SelectionChanged;

    public void SetItems(IEnumerable<SettingsNavigatorItem> items)
    {
        _listBox.BeginUpdate();
        _listBox.Items.Clear();
        _listBox.Items.AddRange(items.Cast<object>().ToArray());
        _listBox.EndUpdate();
    }

    private void UpdateScale()
    {
        _scale = _listBox.DeviceDpi / 96f;
        _listBox.ItemHeight = (int) Math.Round(ITEM_HEIGHT * _scale);
        _card.Padding = new Padding((int) Math.Round(CARD_PADDING * _scale));
        foreach (var icon in _icons.Values)
        {
            icon?.Dispose();
        }
        _icons.Clear();
        _card.Invalidate(true);
    }

    private void SetHoverIndex(int index)
    {
        if (index != _hoverIndex)
        {
            InvalidateItem(_hoverIndex);
            _hoverIndex = index;
            InvalidateItem(_hoverIndex);
        }
    }

    private void InvalidateItem(int index)
    {
        if (index >= 0 && index < _listBox.Items.Count)
        {
            _listBox.Invalidate(_listBox.GetItemRectangle(index));
        }
    }

    private Image? GetIcon(string iconName)
    {
        if (!_icons.TryGetValue(iconName, out var icon))
        {
            icon = EtoPlatform.Current.IconProvider.GetIcon(iconName, IconScenario.Inline16, _scale)?.ToSD();
            _icons[iconName] = icon;
        }
        return icon;
    }

    private void PaintItems(PaintEventArgs e)
    {
        var g = e.Graphics;
        using (var backBrush = new SolidBrush(_listBox.BackColor))
        {
            g.FillRectangle(backBrush, e.ClipRectangle);
        }
        for (int i = _listBox.TopIndex; i < _listBox.Items.Count; i++)
        {
            var bounds = _listBox.GetItemRectangle(i);
            if (bounds.Top > _listBox.ClientSize.Height) break;
            if (bounds.IntersectsWith(e.ClipRectangle))
            {
                PaintItem(g, i, bounds);
            }
        }
    }

    private void PaintItem(Graphics g, int index, Rectangle bounds)
    {
        var item = (SettingsNavigatorItem) _listBox.Items[index];
        var margin = (int) Math.Round(ITEM_MARGIN * _scale);
        var itemRect = Rectangle.Inflate(bounds, -margin, -margin);
        bool selected = index == _listBox.SelectedIndex;
        bool hover = index == _hoverIndex;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (selected || hover)
        {
            // Overlay the foreground color so this works in both light and dark mode
            using var highlightBrush = new SolidBrush(Color.FromArgb(selected ? 20 : 10, _listBox.ForeColor));
            using var path = CreateRoundedRect(itemRect, ITEM_CORNER_RADIUS * _scale);
            g.FillPath(highlightBrush, path);
        }
        if (selected)
        {
            var barHeight = itemRect.Height / 2f;
            var barRect = new RectangleF(itemRect.X, itemRect.Y + (itemRect.Height - barHeight) / 2,
                3 * _scale, barHeight);
            using var accentBrush = new SolidBrush(SystemColors.Highlight);
            using var path = CreateRoundedRect(barRect, 1.5f * _scale);
            g.FillPath(accentBrush, path);
        }
        if (selected && _listBox is { Focused: true, ShouldShowFocusCues: true })
        {
            // Draw the keyboard focus outline just outside the highlight
            var focusWidth = Math.Max(1, (int) _scale);
            var focusRect = RectangleF.Inflate(itemRect, focusWidth / 2f, focusWidth / 2f);
            using var focusPen = new Pen(_listBox.ForeColor, focusWidth);
            using var path = CreateRoundedRect(focusRect, ITEM_CORNER_RADIUS * _scale + focusWidth / 2f);
            g.DrawPath(focusPen, path);
        }
        g.SmoothingMode = SmoothingMode.Default;

        var x = itemRect.X + (int) Math.Round(10 * _scale);
        var iconSize = (int) Math.Round(ICON_SIZE * _scale);
        var icon = GetIcon(item.IconName);
        if (icon != null)
        {
            g.DrawImage(icon, x, itemRect.Y + (itemRect.Height - iconSize) / 2, iconSize, iconSize);
        }
        x += iconSize + (int) Math.Round(10 * _scale);

        var textRect = itemRect with { X = x, Width = itemRect.Right - x };
        TextRenderer.DrawText(g, item.Label, _listBox.Font, textRect, _listBox.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);
    }

    private void PaintCard(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(_card.BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new RectangleF(0.5f, 0.5f, _card.Width - 1, _card.Height - 1);
        using var path = CreateRoundedRect(rect, CARD_CORNER_RADIUS * _scale);
        using var fillBrush = new SolidBrush(_listBox.BackColor);
        g.FillPath(fillBrush, path);
        using var borderPen = new Pen(Color.FromArgb(40, _listBox.ForeColor));
        g.DrawPath(borderPen, path);
    }

    private static GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private class NavigatorListBox : ListBox
    {
        public bool ShouldShowFocusCues => ShowFocusCues;
    }
}