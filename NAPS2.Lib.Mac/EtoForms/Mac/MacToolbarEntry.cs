using Eto.Forms;
using Eto.Mac;
using Eto.Mac.Forms.ToolBar;

namespace NAPS2.EtoForms.Mac;

public record MacToolbarEntry(string Identifier, NSToolbarItem? Item)
{
    public static NSToolbarItem CreateSeparator()
    {
        return new ToolBarHandler.DividerToolbarItem(true);
    }

    public static NSToolbarItem CreateItem(Command command, string? title = null, string? tooltip = null, bool nav = false)
    {
        return new NSToolbarItem
        {
            Image = command.Image?.ToNS(),
            Title = title ?? "",
            Label = command.ToolBarText ?? "",
            ToolTip = tooltip ?? command.ToolBarText ?? "",
            Bordered = true,
            Navigational = nav
        }.WithAction(command.Execute);
    }

    public static NSToolbarItem CreateMenuItem(Command menuCommand, MenuProvider menuProvider,
        string? title = null, string? tooltip = null)
    {
        return new NSMenuToolbarItem
        {
            Image = menuCommand.Image?.ToNS(),
            Label = menuCommand.ToolBarText ?? "",
            Title = title ?? "",
            ToolTip = tooltip ?? menuCommand.ToolBarText ?? "",
            Menu = CreateMenu(menuProvider)
        };
    }

    private static NSMenu CreateMenu(MenuProvider menuProvider)
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
}