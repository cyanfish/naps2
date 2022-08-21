using System.Xml.Serialization;

namespace NAPS2.Config;

/// <summary>
/// Adds ordering metadata to config properties to ensure the XML output order is the same as the source code order.
/// </summary>
public abstract class ConfigPropAttribute : XmlElementAttribute
{
    protected ConfigPropAttribute(int line)
    {
        // This only works because NAPS2.Serialization.XmlSerializer works differently than the .NET serializer.
        // With .NET this would break deserialization as it enforces the input is in the correct order, which would
        // change as we add new properties.
        Order = line;
        // With .NET we would also need to set IsNullable to true to ensure all nulls are included in the output.
    }
}