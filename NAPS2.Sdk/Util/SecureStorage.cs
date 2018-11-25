using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NAPS2.Util
{
    /// <summary>
    /// A class to help encrypt and decrypt passwords/tokens using the ProtectedData API.
    ///
    /// The encryption is tied to the current user so other users can't steal credentials.
    /// It is potentially vulnerable to malicious applications running under the same user.
    /// </summary>
    public class SecureStorage
    {
        public static Lazy<RNGCryptoServiceProvider> CryptoRandom { get; } = new Lazy<RNGCryptoServiceProvider>();

        public static string Encrypt(string plaintext)
        {
            if (plaintext == null)
            {
                return null;
            }
            byte[] salt = new byte[8];
            CryptoRandom.Value.GetBytes(salt);
            // TODO: This won't work for portable...
            byte[] ciphertext = ProtectedData.Protect(Encoding.UTF8.GetBytes(plaintext), salt, DataProtectionScope.CurrentUser);
            return $"encrypted-{Convert.ToBase64String(salt)}-{Convert.ToBase64String(ciphertext)}";
        }

        public static string Decrypt(string coded)
        {
            if (coded == null)
            {
                return null;
            }
            string[] parts = coded.Split('-');
            if (parts.Length != 3 || parts[0] != "encrypted")
            {
                // Not a coded string
                return coded;
            }
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] ciphertext = Convert.FromBase64String(parts[2]);
            string plaintext = Encoding.UTF8.GetString(ProtectedData.Unprotect(ciphertext, salt, DataProtectionScope.CurrentUser));
            return plaintext;
        }
    }
}
