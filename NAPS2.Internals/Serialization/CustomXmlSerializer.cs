namespace NAPS2.Serialization;

public abstract class CustomXmlSerializer
{
    public abstract void SerializeObject(object obj, XElement element, Type type);

    public abstract object DeserializeObject(XElement element, Type type);
}

public abstract class CustomXmlSerializer<T> : CustomXmlSerializer where T : notnull
{
    public override void SerializeObject(object obj, XElement element, Type type)
    {
        Serialize((T)obj, element);
    }

    protected abstract void Serialize(T obj, XElement element);

    public override object DeserializeObject(XElement element, Type type)
    {
        return Deserialize(element);
    }

    protected abstract T Deserialize(XElement element);
}