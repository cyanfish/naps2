namespace NAPS2.Serialization;

public abstract class CustomXmlTypes
{
    public abstract Type[] GetKnownTypes(Type baseType);
}

public abstract class CustomXmlTypes<T> : CustomXmlTypes
{
    public override Type[] GetKnownTypes(Type baseType) => GetKnownTypes();

    protected abstract Type[] GetKnownTypes();
}