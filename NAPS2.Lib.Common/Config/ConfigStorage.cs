using System.Linq.Expressions;
using System.Reflection;

namespace NAPS2.Config;

public class ConfigStorage<TConfig>
{
    private readonly StorageNode _root = new() { IsRoot = true };

    public ConfigStorage()
    {
    }

    public ConfigStorage(TConfig initialValues)
    {
        CopyObjectToNode(initialValues, _root);
    }

    public bool TryGet<T>(Expression<Func<TConfig, T>> accessor, out T value)
    {
        lock (this)
        {
            var node = GetNode(accessor);
            if (!node.IsLeaf)
            {
                throw new ArgumentException("You can't access a child config directly on a ConfigScope");
            }
            if (!node.HasValue)
            {
                value = default!;
                return false;
            }
            value = (T) node.Value!;
            return true;
        }
    }

    public void Set<T>(Expression<Func<TConfig, T>> accessor, T value)
    {
        lock (this)
        {
            var node = GetNode(accessor)!;
            CopyObjectToNode(value, node);
        }
    }

    public void Remove<T>(Expression<Func<TConfig, T>> accessor)
    {
        lock (this)
        {
            var node = GetNode(accessor);
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

    private StorageNode GetNode<T>(Expression<Func<TConfig, T>> accessor)
    {
        var (lookup, _) = ExpandExpression(accessor);
        return GetNodeRecursive(_root, lookup);
    }

    private StorageNode GetNodeRecursive(StorageNode node, LookupNode? lookup)
    {
        if (lookup == null)
        {
            return node;
        }
        var nextNode = node.Children.GetOrSet(lookup.Key, () => new StorageNode
        {
            Key = lookup.Key,
            Parent = node,
            IsLeaf = !lookup.IsChildConfig,
            ValueType = lookup.ValueType
        });
        return GetNodeRecursive(nextNode, lookup.Next);
    }

    private (LookupNode? head, LookupNode? tail) ExpandExpression(Expression expression)
    {
        switch (expression)
        {
            case LambdaExpression lambdaExpression:
                return ExpandExpression(lambdaExpression.Body);
            case MemberExpression memberExpression:
                var propInfo = memberExpression.Member as PropertyInfo ?? throw new ArgumentException();
                var key = propInfo.Name;
                var isChildConfig = IsChildProp(propInfo);
                var node = new LookupNode(key, isChildConfig, propInfo.PropertyType);
                var (head, tail) = ExpandExpression(memberExpression.Expression!);
                if (tail == null)
                {
                    return (node, node);
                }
                tail.Next = node;
                return (head, node);
            case ParameterExpression:
                return (null, null);
            default:
                throw new ArgumentException();
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
            throw new ArgumentException("A [Child] config can't be set to null");
        }

        // TODO: Consider adding a non-generic class that statically stores this cache
        var propData = node.ValueType
            .GetProperties()
            .Select(x => (x, IsChildProp(x)))
            .ToArray();
        foreach (var (prop, isChild) in propData)
        {
            var childObj = prop.GetValue(obj);
            var childNode = node.Children.GetOrSet(prop.Name, () => new StorageNode
            {
                Key = prop.Name,
                Parent = node,
                ValueType = prop.PropertyType,
                IsLeaf = !isChild
            });
            CopyObjectToNode(childObj, childNode);
        }
    }

    private static bool IsChildProp(MemberInfo x)
    {
        return x.GetCustomAttributes().Any(y => y is ChildAttribute);
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

    private record LookupNode(string Key, bool IsChildConfig, Type ValueType)
    {
        public LookupNode? Next { get; set; }
    }
}