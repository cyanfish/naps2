using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace NAPS2.Serialization;

/// <summary>
/// A class to help encrypt and decrypt passwords/tokens using the ProtectedData API.
///
/// The encryption is tied to the current user so other users can't steal credentials.
/// It is potentially vulnerable to malicious applications running under the same user.
/// </summary>
public static class SecureStorage
{
    public static Lazy<RandomNumberGenerator> CryptoRandom { get; } = new(RandomNumberGenerator.Create);

    [return: NotNullIfNotNull("plaintext")]
    public static string? Encrypt(string plaintext)
    {
#if STANDALONE
        return plaintext;
#else
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) return plaintext;
#endif
        if (plaintext == null)
        {
            return null;
        }
        byte[] salt = new byte[8];
        CryptoRandom.Value.GetBytes(salt);
        var encoded = Encoding.UTF8.GetBytes(plaintext);
        byte[] ciphertext = ProtectedData.Protect(encoded, salt, DataProtectionScope.CurrentUser);
        return $"encrypted-{Convert.ToBase64String(salt)}-{Convert.ToBase64String(ciphertext)}";
#endif
    }

    [return: NotNullIfNotNull("coded")]
    public static string? Decrypt(string coded)
    {
#if STANDALONE
        return coded;
#else
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindowsVersionAtLeast(7)) return coded;
#endif
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
        byte[] decrypted = ProtectedData.Unprotect(ciphertext, salt, DataProtectionScope.CurrentUser);
        string plaintext = Encoding.UTF8.GetString(decrypted);
        return plaintext;
#endif
    }
}