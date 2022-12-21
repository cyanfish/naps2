namespace NAPS2.EtoForms.Mac;

public class MacToolbarDelegate : NSToolbarDelegate
{
    private readonly string[] _identifiers;
    private readonly Dictionary<string, NSToolbarItem> _itemMap;

    public MacToolbarDelegate(List<NSToolbarItem?> items)
    {
        _identifiers = items.Select(x => x?.Identifier ?? NSToolbar.NSToolbarSpaceItemIdentifier).ToArray();
        _itemMap = items.WhereNotNull().ToDictionary(x => x.Identifier);
    }

    public override string[] AllowedItemIdentifiers(NSToolbar toolbar) => _identifiers;
    public override string[] DefaultItemIdentifiers(NSToolbar toolbar) => _identifiers;
    public override string[] SelectableItemIdentifiers(NSToolbar toolbar) => Array.Empty<string>();

    public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
    {
        return _itemMap.Get(itemIdentifier);
    }
}