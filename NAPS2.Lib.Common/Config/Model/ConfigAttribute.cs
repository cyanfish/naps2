namespace NAPS2.Config.Model;

/// <summary>
/// Annotates a type or property as being a "config" object. Config objects can't be null, and can contain a mix of
/// normal properties and nested "config" properties (that have [Config] on the property or type). Config objects also
/// need to be serializable (i.e. have a parameterless constructor, and properties must have setters accessible by
/// reflection).
/// </summary>
public class ConfigAttribute : Attribute
{
}