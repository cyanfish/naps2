using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Serialization;

public class SecureStorageTests
{
    [Fact]
    public void EncryptAndDecrypt()
    {
        var text = "Hello, world!";
        var encrypted = SecureStorage.Encrypt(text);
        Assert.StartsWith("encrypted-", encrypted);
        Assert.DoesNotContain("Hello", encrypted);

        var decrypted = SecureStorage.Decrypt(encrypted);
        Assert.Equal(text, decrypted);
    }
}