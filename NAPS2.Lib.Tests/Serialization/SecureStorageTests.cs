using NAPS2.Sdk.Tests;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Lib.Tests.Serialization;

public class SecureStorageTests
{
    [PlatformFact(include: PlatformFlags.Windows)]
    public void EncryptAndDecrypt()
    {
        var text = "Hello, world!";
        var encrypted = SecureStorage.Encrypt(text);
        Assert.StartsWith("encrypted-", encrypted);
        Assert.DoesNotContain("Hello", encrypted);

        var decrypted = SecureStorage.Decrypt(encrypted);
        Assert.Equal(text, decrypted);
    }
    
    [PlatformFact(exclude: PlatformFlags.Windows)]
    public void EncryptAndDecryptNonWindows()
    {
        var text = "Hello, world!";
        var encrypted = SecureStorage.Encrypt(text);
        Assert.Equal(text, encrypted);

        var decrypted = SecureStorage.Decrypt(encrypted);
        Assert.Equal(text, decrypted);
    }
}