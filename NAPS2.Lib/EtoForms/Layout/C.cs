using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Layout;

public static class C
{
    /// <summary>
    /// Creates a label with wrapping disabled. For WinForms support, all labels must not wrap.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Label NoWrap(string text) =>
        new Label { Text = text, Wrap = WrapMode.None };

    /// <summary>
    /// Creates a link button with the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static LinkButton Link(string text)
    {
        var link = new LinkButton { Text = text };
        if (EtoPlatform.Current.IsWinForms)
        {
            // TODO: Remove this when https://github.com/dotnet/winforms/issues/11935 is fixed
            link.TextColor = EtoPlatform.Current.ColorScheme.LinkColor;
        }
        return link;
    }

    /// <summary>
    /// Creates a link button with the specified text and action.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    public static LinkButton Link(string text, Action onClick)
    {
        var link = Link(text);
        link.Command = new ActionCommand(onClick);
        return link;
    }

    /// <summary>
    /// Creates a link button with the given URL as both text and click action.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public static LinkButton UrlLink(string url, string? label = null)
    {
        void OnClick() => ProcessHelper.OpenUrl(url);
        return Link(label ?? url, OnClick);
    }

    /// <summary>
    /// Creates a button with the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Button Button(string text) => new() { Text = text };

    /// <summary>
    /// Creates a button with the specified text and action.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    public static Button Button(string text, Action onClick) =>
        Button(text, new ActionCommand(onClick));

    /// <summary>
    /// Creates a button with the specified text and command.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    public static Button Button(string text, ICommand command) =>
        new Button
        {
            Text = text,
            Command = command
        };

    public static Button Button(ActionCommand command) =>
        Button(command.MenuText, command);

    public static Button Button(ActionCommand command, ButtonImagePosition imagePosition, ButtonFlags flags = default)
    {
        return Button(command, command.IconName, imagePosition, flags);
    }

    public static Button Button(ActionCommand command, string? iconName, ButtonImagePosition imagePosition = default, ButtonFlags flags = default)
    {
        var button = Button(command);
        if (command.Image != null)
        {
            EtoPlatform.Current.AttachDpiDependency(button,
                scale =>
                {
                    int targetSize = flags.HasFlag(ButtonFlags.LargeIcon) ? 32 : 16;
                    button.Image = command.Image.Clone().ResizeTo((int) (targetSize * scale));
                });
        }
        else if (iconName != null)
        {
            bool oversized = imagePosition == ButtonImagePosition.Above && flags.HasFlag(ButtonFlags.LargeIcon);
            EtoPlatform.Current.AttachDpiDependency(button,
                scale => button.Image = EtoPlatform.Current.IconProvider.GetIcon(iconName, scale, oversized));
        }
        button.ImagePosition = imagePosition;
        if (flags.HasFlag(ButtonFlags.LargeText))
        {
            EtoPlatform.Current.AttachDpiDependency(button,
                scale => button.Font = new Font(button.Font.Family, 12 * scale));
        }
        EtoPlatform.Current.ConfigureImageButton(button, flags);
        return button;
    }

    // TODO: Clean up button overloads
    public static Button ImageButton(Command command) =>
        new Button
        {
            Text = command.MenuText,
            Command = command,
            Image = command.Image
        };

    /// <summary>
    /// Creates a null placeholder for Eto layouts that absorbs scaling.
    /// </summary>
    /// <returns></returns>
    public static LayoutControl Filler() =>
        new LayoutControl(null).Scale();

    /// <summary>
    /// Creates a null placeholder for Eto layouts.
    /// </summary>
    /// <returns></returns>
    public static LayoutControl Spacer() =>
        new LayoutControl(null);

    /// <summary>
    /// Creates an label of default height to be used as a visual paragraph separator.
    /// </summary>
    /// <returns></returns>
    public static LayoutElement TextSpace() => NoWrap(" ");

    public static Label Label(string text) => new() { Text = text };

    public static DropDown DropDown()
    {
        var dropDown = new DropDown();
        EtoPlatform.Current.ConfigureDropDown(dropDown);
        return dropDown;
    }

    public static CheckBox CheckBox(string text) => new() { Text = text };

    public static Button CancelButton(Dialog dialog, string? text = null) =>
        DialogButton(dialog, text ?? UiStrings.Cancel, isAbort: true);

    public static Button OkButton(Dialog dialog, Action? beforeClose = null, string? text = null) =>
        DialogButton(dialog, text ?? UiStrings.OK, isDefault: true, beforeClose: beforeClose);

    public static Button OkButton(Dialog dialog, Func<bool>? beforeCloseWithCancel, string? text = null) =>
        DialogButton(dialog, text ?? UiStrings.OK, isDefault: true, beforeCloseWithCancel: beforeCloseWithCancel);

    public static Button DialogButton(Dialog dialog, string text, bool isDefault = false, bool isAbort = false,
        Action? beforeClose = null, Func<bool>? beforeCloseWithCancel = null)
    {
        var button = Button(text, () =>
        {
            if (!(beforeCloseWithCancel?.Invoke() ?? true))
            {
                return;
            }
            beforeClose?.Invoke();
            dialog.Close();
        });
        if (isDefault)
        {
            dialog.DefaultButton = button;
        }
        if (isAbort)
        {
            dialog.AbortButton = button;
        }
        return button;
    }

    private static IEnumerable<Control> GetAllControls(Control control)
    {
        if (control is not Container container) return Enumerable.Repeat(control, 1);
        return container.Controls.SelectMany(GetAllControls).Append(control);
    }

    public static LayoutElement None()
    {
        return new SkipLayoutElement();
    }

    public static Button IconButton(string iconName, Action onClick)
    {
        var button = new Button
        {
            ImagePosition = ButtonImagePosition.Overlay
        };
        EtoPlatform.Current.AttachDpiDependency(button, scale =>
        {
            var icon = EtoPlatform.Current.IconProvider.GetIcon(iconName, scale)!;
            button.Image = icon;
            button.MinimumSize = new Size(icon.Width + 30, 0);
        });
        button.Click += (_, _) => onClick();
        return button;
    }

    public static MenuItem ButtonMenuItem(Window window, ActionCommand command)
    {
        var menuItem = new ButtonMenuItem(command);
        EtoPlatform.Current.AttachDpiDependency(window, scale =>
        {
            EtoPlatform.Current.SetImageSize(menuItem, (int) (16 * scale));
            menuItem.Image = command.GetIconImage(scale);
        });
        return menuItem;
    }
}