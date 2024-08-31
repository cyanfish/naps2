using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public class LinkNotificationView : NotificationView
{
    private readonly string? _linkTarget;
    private readonly string? _folderTarget;

    private readonly Label _label = new();
    private readonly ContextMenu _contextMenu = new();
    private readonly LinkButton _link;

    protected LinkNotificationView(
        NotificationModel model, string title, string linkLabel, string? linkTarget, string? folderTarget)
        : base(model)
    {
        _label.Text = title;
        _label.Font = new Font(_label.Font.Family, _label.Font.Size, FontStyle.Bold);
        _link = C.Link(linkLabel);
        _linkTarget = linkTarget;
        _folderTarget = folderTarget;

        if (_folderTarget != null)
        {
            _contextMenu.Items.Add(new ActionCommand(OpenFolder) { Text = UiStrings.OpenFolder });
        }

        _link.Click += (_, _) => LinkClick();
        _link.MouseUp += (_, args) =>
        {
            if (args.Buttons == MouseButtons.Alternate)
            {
                LinkRightClick();
            }
        };
    }

    protected override void BeforeCreateContent()
    {
        _label.BackgroundColor = _link.BackgroundColor = BackgroundColor;
    }

    protected override LayoutElement PrimaryContent => _label.DynamicWrap(180).MaxWidth(180).Scale();

    protected override LayoutElement SecondaryContent => _link;

    protected virtual void LinkClick()
    {
        ProcessHelper.OpenFile(_linkTarget!);
    }

    protected virtual void LinkRightClick()
    {
        if (_folderTarget != null)
        {
            _contextMenu.Show(_link);
        }
    }

    protected virtual void OpenFolder()
    {
        ProcessHelper.OpenFolder(_folderTarget!);
    }
}