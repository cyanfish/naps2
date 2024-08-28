using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms.ToolBar;

namespace NAPS2.EtoForms.Mac;

public static class MacToolbarItems
{
    public static NSToolbarItem CreateSeparator(string id)
    {
        return new ToolBarHandler.DividerToolbarItem(true);
    }

    public static NSToolbarItem Create(string id, ActionCommand command, string? title = null, string? tooltip = null,
        bool nav = false)
    {
        var item = new NSToolbarItem(id)
        {
            Image = command.GetIconImage(1)?.ToNS(),
            Title = title ?? "",
            Label = command.ToolBarText ?? "",
            // TODO: Verify this fixes label display on macOS 10.15
            ToolTip = OperatingSystem.IsMacOSVersionAtLeast(11) ? tooltip ?? command.ToolBarText ?? "" : "",
            Bordered = true,
            Autovalidates = false
        }.WithAction(command.Execute);
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            item.Navigational = nav;
        }
        return item;
    }

    public static NSToolbarItem CreateMenu(string id, ActionCommand menuCommand, MenuProvider menuProvider,
        string? title = null, string? tooltip = null)
    {
        return new NSMenuToolbarItem(id)
        {
            Image = menuCommand.GetIconImage(1)?.ToNS(),
            Label = menuCommand.ToolBarText ?? "",
            Title = title ?? "",
            ToolTip = tooltip ?? menuCommand.ToolBarText ?? "",
            Menu = CreateMenuObject(menuProvider)
        };
    }

    private static NSMenu CreateMenuObject(MenuProvider menuProvider)
    {
        var menu = new NSMenu();
        menuProvider.Handle(items =>
        {
            menu.RemoveAllItems();
            foreach (var item in items)
            {
                switch (item)
                {
                    case MenuProvider.CommandItem { Command: var command }:
                        menu.AddItem(new NSMenuItem
                        {
                            Title = command.MenuText,
                            Image = command.Image?.ToNS()
                        }.WithAction(command.Execute));
                        break;
                    case MenuProvider.SeparatorItem:
                        menu.AddItem(NSMenuItem.SeparatorItem);
                        break;
                    case MenuProvider.SubMenuItem:
                        throw new NotImplementedException();
                }
            }
        });
        return menu;
    }

    public static NSToolbarItem CreateSpace()
    {
        return null!;
    }
}