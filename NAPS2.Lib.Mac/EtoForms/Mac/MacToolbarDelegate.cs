using Eto.Mac.Forms.ToolBar;

namespace NAPS2.EtoForms.Mac;

public class MacToolbarDelegate : NSToolbarDelegate
{
    private readonly string[] _identifiers;
    private string[] _selectableIdentifiers;
    private readonly Dictionary<string, NSToolbarItem> _itemMap;

    public MacToolbarDelegate(List<NSToolbarItem> items)
    {
        _identifiers = items.Select(x => x.Identifier).ToArray();
        _selectableIdentifiers = items.Where(x => x is not ToolBarHandler.DividerToolbarItem)
            .Select(x => x.Identifier).ToArray();
        _itemMap = items.ToDictionary(x => x.Identifier);
    }

    public NativeHandle Handle { get; }

    public void Dispose()
    {
    }

    public override string[] AllowedItemIdentifiers(NSToolbar toolbar) => _identifiers;
    public override string[] DefaultItemIdentifiers(NSToolbar toolbar) => _identifiers;
    public override string[] SelectableItemIdentifiers(NSToolbar toolbar) => _selectableIdentifiers;

    public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
    {
        return _itemMap.Get(itemIdentifier);
    }
}