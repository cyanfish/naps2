using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NAPS2.Util;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class OauthToken : IXmlSerializable
    {
        private string accessToken;
        private string accessTokenEncrypted;
        private string refreshToken;
        private string refreshTokenEncrypted;
        
        public string AccessToken
        {
            get
            {
                accessToken = accessToken ?? SecureStorage.Decrypt(accessTokenEncrypted);
                return accessToken;
            }
            set
            {
                accessToken = value;
                accessTokenEncrypted = null;
            }
        }

        public string RefreshToken
        {
            get
            {
                refreshToken = refreshToken ?? SecureStorage.Decrypt(refreshTokenEncrypted);
                return refreshToken;
            }
            set
            {
                refreshToken = value;
                refreshTokenEncrypted = null;
            }
        }

        public DateTime Expiry { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            accessTokenEncrypted = reader.ReadElementString();
            refreshTokenEncrypted = reader.ReadElementString();
            Expiry = DateTime.Parse(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            accessTokenEncrypted = accessTokenEncrypted ?? SecureStorage.Encrypt(accessToken);
            refreshTokenEncrypted = refreshTokenEncrypted ?? SecureStorage.Encrypt(accessToken);
            writer.WriteElementString("AccessToken", accessTokenEncrypted);
            writer.WriteElementString("RefreshToken", refreshTokenEncrypted);
            writer.WriteElementString("Expiry", Expiry.ToString("O"));
        }
    }
}