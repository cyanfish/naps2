namespace NAPS2.Serialization;

public class UntypedXmlSerializer : XmlSerializer
{
    public XElement SerializeToXElement(Type type, object? obj, string elementName)
    {
        return SerializeInternal(obj, new XElement(elementName), type);
    }

    public XElement SerializeToXElement(Type type, object? obj, XElement element)
    {
        return SerializeInternal(obj, element, type);
    }

    public object? DeserializeFromXElement(Type type, XElement element)
    {
        return DeserializeInternal(element, type);
    }
}