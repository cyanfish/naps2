using System.Linq.Expressions;
using NAPS2.Serialization;

namespace NAPS2.Config.Model;

public class ConfigStorage<TConfig>
{
    private readonly StorageNode _root = new()
    {
        IsRoot = true,
        ValueType = typeof(TConfig),
        IsLeaf = !ConfigLookup.HasConfigAttribute(typeof(TConfig))
    };

    public ConfigStorage()
    {
    }

    public ConfigStorage(TConfig initialValues)
    {
        CopyObjectToNode(initialValues, _root);
    }

    public bool TryGet<T>(Expression<Func<TConfig, T>> accessor, out T value)
    {
        var result = TryGet(ConfigLookup.ExpandExpression(accessor), out var obj);
        value = (T) obj;
        return result;
    }
    
    public bool TryGet(ConfigLookup lookup, out object? value)
    {
        lock (this)
        {
            var node = GetNode(lookup);
            if (!node.IsLeaf)
            {
                throw new ArgumentException("You can't access a child config directly on a ConfigScope");
            }
            if (!node.HasValue)
            {
                value = default!;
                return false;
            }
            value = node.Value!;
            return true;
        }
    }

    public void Set<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        lock (this)
        {
            var node = GetNode(ConfigLookup.ExpandExpression(accessor));
            CopyObjectToNode(value, node);
        }
    }

    public void Remove<T>(Expression<Func<TConfig, T>> accessor)
    {
        lock (this)
        {
            var node = GetNode(ConfigLookup.ExpandExpression(accessor));
            if (node.IsRoot)
            {
                node.Children.Clear();
            }
            else
            {
                node.Parent!.Children.Remove(node.Key);
            }
        }
    }

    private StorageNode GetNode(ConfigLookup lookup)
    {
        return GetNodeRecursive(_root, lookup.Head);
    }

    public void CopyFrom(ConfigStorage<TConfig> source)
    {
        // TODO: Any deadlock concerns?
        // TODO: Or in general, maybe we don't need to lock ConfigStorage as it happens on a higher level...
        lock (this)
        {
            lock (source)
            {
                CopyNodeToNode(source._root, _root);
            }
        }
    }

    private StorageNode GetNodeRecursive(StorageNode node, ConfigLookup.Node lookup)
    {
        var nextLookup = lookup.Next;
        if (nextLookup == null)
        {
            return node;
        }
        var nextNode = node.Children.GetOrSet(nextLookup.Key, () => new StorageNode
        {
            Key = nextLookup.Key,
            Parent = node,
            IsLeaf = nextLookup.IsLeaf,
            ValueType = nextLookup.Type
        });
        return GetNodeRecursive(nextNode, nextLookup);
    }

    private void CopyNodeToNode(StorageNode src, StorageNode dst)
    {
        if (src.IsLeaf)
        {
            if (src.HasValue)
            {
                dst.HasValue = true;
                dst.Value = src.Value;
            }
            return;
        }

        foreach (var childSrcKvp in src.Children)
        {
            var childSrc = childSrcKvp.Value;
            var childDst = dst.Children.GetOrSet(childSrc.Key, () => new StorageNode
            {
                Key = childSrc.Key,
                Parent = dst,
                ValueType = childSrc.ValueType,
                IsLeaf = childSrc.IsLeaf,
            });
            CopyNodeToNode(childSrc, childDst);
        }
    }
    
    private void CopyObjectToNode(object? obj, StorageNode node)
    {
        if (node.IsLeaf)
        {
            node.Value = obj;
            node.HasValue = true;
            return;
        }
        if (obj == null)
        {
            throw new ArgumentException("A config can't be set to null");
        }

        // TODO: We should probably detect and throw an exception for cycles rather than stack overflowing
        foreach (var propData in ConfigLookup.GetPropertyData(node.ValueType))
        {
            var childObj = propData.PropertyInfo.GetValue(obj);
            var childNode = node.Children.GetOrSet(propData.Name, () => new StorageNode
            {
                Key = propData.Name,
                Parent = node,
                ValueType = propData.Type,
                IsLeaf = !propData.IsNestedConfig
            });
            CopyObjectToNode(childObj, childNode);
        }
    }

    private void CopyNodeToXElement(StorageNode src, XElement dst, UntypedXmlSerializer serializer)
    {
        if (src.IsLeaf)
        {
            if (src.HasValue)
            {
                serializer.SerializeToXElement(src.ValueType, src.Value, dst);
            }
            else
            {
                dst.Remove();
            }
            return;
        }
        foreach (var childNodeKvp in src.Children)
        {
            var childNode = childNodeKvp.Value;
            var childElement = new XElement(childNode.Key);
            dst.Add(childElement);
            CopyNodeToXElement(childNode, childElement, serializer);
        }
    }

    private void CopyXElementToNode(XElement src, StorageNode dst, UntypedXmlSerializer serializer)
    {
        if (dst.IsLeaf)
        {
            var value = serializer.DeserializeFromXElement(dst.ValueType, src);
            dst.Value = value;
            dst.HasValue = true;
            return;
        }
        var propDataDict = ConfigLookup.GetPropertyData(dst.ValueType).ToDictionary(x => x.Name);
        foreach (var childElement in src.Elements())
        {
            // TODO: Handle errors
            var propData = propDataDict[childElement.Name.ToString()];
            var childNode = dst.Children.GetOrSet(propData.Name, () => new StorageNode
            {
                Key = propData.Name,
                Parent = dst,
                ValueType = propData.Type,
                IsLeaf = !propData.IsNestedConfig
            });
            CopyXElementToNode(childElement, childNode, serializer);
        }
    }

    public void SerializeTo(XDocument doc, string? customRootElementName)
    {
        lock (this)
        {
            if (_root.IsLeaf && !_root.HasValue)
            {
                // A valid XML doc needs a root node so this case makes no sense
                throw new InvalidOperationException("When serializing a plain object, a value must be specified");
            }
            var serializer = new UntypedXmlSerializer();
            var rootElementName = customRootElementName ?? serializer.GetDefaultElementName(typeof(TConfig));
            doc.Add(new XElement(rootElementName));
            CopyNodeToXElement(_root, doc.Root!, serializer);
        }
    }

    public void DeserializeFrom(XDocument doc)
    {
        lock (this)
        {
            CopyXElementToNode(doc.Root!, _root, new UntypedXmlSerializer());
        }
    }

    private class StorageNode
    {
        public bool IsLeaf { get; init; }

        public bool IsRoot { get; init; }

        public string Key { get; init; }

        public object? Value { get; set; }
        
        public bool HasValue { get; set; }

        public Type ValueType { get; init; }

        public StorageNode? Parent { get; init; }

        public Dictionary<string, StorageNode> Children { get; } = new();
    }
}