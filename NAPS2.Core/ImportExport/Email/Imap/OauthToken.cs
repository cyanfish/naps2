using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Imap
{
    public class OauthToken : IXmlSerializable
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime Expiry { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            AccessToken = SecureStorage.Decrypt(reader.ReadElementString());
            RefreshToken = SecureStorage.Decrypt(reader.ReadElementString());
            Expiry = DateTime.Parse(reader.ReadElementString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("AccessToken", SecureStorage.Encrypt(AccessToken));
            writer.WriteElementString("RefreshToken", SecureStorage.Encrypt(RefreshToken));
            writer.WriteElementString("Expiry", Expiry.ToString("O"));
        }
    }
}