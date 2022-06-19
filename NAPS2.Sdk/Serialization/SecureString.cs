namespace NAPS2.Serialization;

/// <summary>
/// A class for strings that are encrypted when XML-serialized.
///
/// Includes implicit conversions to and from System.String.
/// Encryption and decryption is lazy.
/// </summary>
public class SecureString
{
    static SecureString()
    {
        XmlSerializer.RegisterCustomSerializer(new Serializer());
    }
        
    private string? _value;
    private string? _valueEncrypted;

    public SecureString(string value)
    {
        _value = value;
    }

    // ReSharper disable once UnusedMember.Local
    private SecureString()
    {
    }

    public static implicit operator SecureString(string s) => new(s);

    public static implicit operator string(SecureString s) => s.ToString();

    public override string ToString()
    {
        _value ??= SecureStorage.Decrypt(_valueEncrypted!);
        return _value;
    }

    private class Serializer : CustomXmlSerializer<SecureString>
    {
        protected override void Serialize(SecureString obj, XElement element)
        {
            element.Value = obj._valueEncrypted ?? SecureStorage.Encrypt(obj._value!);
        }

        protected override SecureString Deserialize(XElement element)
        {
            return new SecureString { _valueEncrypted = element.Value };
        }
    }
}