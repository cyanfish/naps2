using System.Xml.Linq;

namespace NAPS2.Serialization
{
    /// <summary>
    /// A class for strings that are encrypted when XML-serialized.
    ///
    /// Includes implicit conversions to and from System.String.
    /// Encryption and decryption is lazy.
    /// </summary>
    public class SecureString
    {
        private string value;
        private string valueEncrypted;

        public SecureString(string value)
        {
            this.value = value;
        }

        // ReSharper disable once UnusedMember.Local
        private SecureString()
        {
        }

        public static implicit operator SecureString(string s) => new SecureString(s);

        public static implicit operator string(SecureString s) => s.ToString();

        public override string ToString()
        {
            value = value ?? SecureStorage.Decrypt(valueEncrypted);
            return value;
        }

        // ReSharper disable once UnusedMember.Local
        private class Serializer : CustomXmlSerializer<SecureString>
        {
            protected override void Serialize(SecureString obj, XElement element)
            {
                element.Value = obj.valueEncrypted ?? SecureStorage.Encrypt(obj.value);
            }

            protected override SecureString Deserialize(XElement element)
            {
                return new SecureString { valueEncrypted = element.Value };
            }
        }
    }
}
