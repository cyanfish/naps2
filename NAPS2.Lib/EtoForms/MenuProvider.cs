using Eto.Forms;

namespace NAPS2.EtoForms;

public class MenuProvider
{
    private readonly List<Item> _items = new();

    public MenuProvider Dynamic(ListProvider<Command> commandListProvider)
    {
        _items.Add(new DynamicItem
        {
            CommandListProvider = commandListProvider ?? throw new ArgumentNullException(nameof(commandListProvider))
        });
        commandListProvider.OnChanged += () => ContentsChanged?.Invoke();
        return this;
    }

    public MenuProvider Append(Command? command)
    {
        if (command == null)
        {
            return this;
        }
        _items.Add(new CommandItem
        {
            Command = command
        });
        return this;
    }

    public MenuProvider Separator()
    {
        _items.Add(new SeparatorItem());
        return this;
    }

    public MenuProvider SubMenu(Command command, MenuProvider subMenu)
    {
        _items.Add(new SubMenuItem
        {
            Command = command ?? throw new ArgumentNullException(nameof(command)),
            MenuProvider = subMenu
        });
        return this;
    }

    private event Action? ContentsChanged;

    public class Item
    {
    }

    private class DynamicItem : Item
    {
        public ListProvider<Command> CommandListProvider { get; init; } = null!;
    }

    public class CommandItem : Item
    {
        public Command Command { get; init; } = null!;
    }

    public class SubMenuItem : Item
    {
        public Command Command { get; init; } = null!;
        public MenuProvider MenuProvider { get; init; } = null!;
    }

    public class SeparatorItem : Item
    {
    }

    private List<Item> GetSubItems()
    {
        var actualItems = new List<Item>();
        Item? lastItem = null;
        foreach (var item in _items)
        {
            if (item is DynamicItem dynamicItem)
            {
                var dynamicItemValues =
                    dynamicItem.CommandListProvider.Value.Select(x => new CommandItem
                {
                    Command = x
                }).ToList();
                if (dynamicItemValues.Any())
                {
                    actualItems.AddRange(dynamicItemValues);
                    lastItem = item;
                }
            }
            else if (item is not SeparatorItem || (lastItem != null && lastItem is not SeparatorItem))
            {
                actualItems.Add(item);
                lastItem = item;
            }
        }
        return actualItems;
    }

    public void Handle(Action<List<Item>> subItemsHandler)
    {
        subItemsHandler(GetSubItems());
        ContentsChanged += () => subItemsHandler(GetSubItems());
    }
}