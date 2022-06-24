using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

public class ConfigLookup
{
    private static readonly Dictionary<Type, ImmutableList<PropertyData>> PropertyDataCache = new();

    public static ImmutableList<PropertyData> GetPropertyData(Type type)
    {
        lock (PropertyDataCache)
        {
            return PropertyDataCache.GetOrSet(type, () =>
                type
                    .GetProperties()
                    .Select(x => new PropertyData(x.Name, x.PropertyType, IsNestedConfigProp(x), x))
                    .ToImmutableList());
        }
    }

    public static ConfigLookup ExpandExpression(Expression expression)
    {
        switch (expression)
        {
            case LambdaExpression lambdaExpression:
                return ExpandExpression(lambdaExpression.Body);
            case MemberExpression memberExpression:
                var propInfo = memberExpression.Member as PropertyInfo ?? throw new ArgumentException();
                var key = propInfo.Name;
                var isLeaf = !IsNestedConfigProp(propInfo);
                var node = new Node(key, isLeaf, propInfo.PropertyType);
                var nestedLookup = ExpandExpression(memberExpression.Expression!);
                VerifyNotLeaf(nestedLookup.Tail);
                nestedLookup.Tail.Next = node;
                return new ConfigLookup(nestedLookup.Head, node);
            case ParameterExpression parameterExpression:
                var rootType = parameterExpression.Type;
                var rootNode = new Node("c", !HasConfigAttribute(rootType), rootType);
                return new ConfigLookup(rootNode, rootNode);
            default:
                throw new ArgumentException();
        }
    }

    private static void VerifyNotLeaf(Node node)
    {
        if (node.IsLeaf)
        {
            throw new ArgumentException(
                "Attempting to query a property of a config type that doesn't have a [Config] attribute.");
        }
    }

    public ConfigLookup(Node head, Node tail)
    {
        Head = head;
        Tail = tail;
    }

    public Node Head { get; }

    public Node Tail { get; }

    public override string ToString()
    {
        return Head.ToString();
    }

    private static bool IsNestedConfigProp(PropertyInfo prop)
    {
        return HasConfigAttribute(prop) || HasConfigAttribute(prop.PropertyType);
    }

    public static bool HasConfigAttribute(MemberInfo member)
    {
        return member.GetCustomAttributes().Any(x => x is ConfigAttribute);
    }

    // TODO: Make these internal?
    public record Node(string Key, bool IsLeaf, Type Type)
    {
        public Node? Next { get; set; }

        // TODO: Maybe a way to do this without deep copies (e.g. smarter linked list / true immutability)
        public Node Copy(out Node tail)
        {
            var node = new Node(Key, IsLeaf, Type);
            tail = node;
            node.Next = Next?.Copy(out tail);
            return node;
        }

        public override string ToString()
        {
            if (Next == null)
            {
                return Key;
            }
            return $"{Key}.{Next}";
        }
    }

    public record PropertyData(string Name, Type Type, bool IsNestedConfig, PropertyInfo PropertyInfo);

    public ConfigLookup Append(PropertyData prop)
    {
        var next = new Node(prop.Name, !prop.IsNestedConfig, prop.Type);
        var head = Head.Copy(out var tail);
        VerifyNotLeaf(tail);
        tail.Next = next;
        return new ConfigLookup(head, next);
    }
}