namespace NAPS2.Config.Model;

internal class NodeTree
{
    private readonly Node _root = new();

    public void Add(ConfigLookup lookup)
    {
        var node = _root;
        for (var lookupNode = lookup.Head; lookupNode != null; lookupNode = lookupNode.Next)
        {
            if (!node.Children.ContainsKey(lookupNode.Key))
            {
                node.Children.Add(lookupNode.Key, new());
            }
            node = node.Children[lookupNode.Key];
        }
        node.IsPresent = true;
    }

    public bool ContainsPrefix(ConfigLookup lookup)
    {
        var node = _root;
        for (var lookupNode = lookup.Head; lookupNode != null; lookupNode = lookupNode.Next)
        {
            if (node.IsPresent)
            {
                return true;
            }
            if (!node.Children.ContainsKey(lookupNode.Key))
            {
                return false;
            }
            node = node.Children[lookupNode.Key];
        }
        return node.IsPresent;
    }

    private class Node
    {
        public bool IsPresent { get; set; }
        public Dictionary<string, Node> Children { get; } = new();
    }
}