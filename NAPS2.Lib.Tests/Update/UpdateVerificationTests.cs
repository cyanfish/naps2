using System.Security.Cryptography;
using NAPS2.Sdk.Tests;
using NAPS2.Update;
using Xunit;

namespace NAPS2.Lib.Tests.Update;

public class UpdateVerificationTests : ContextualTests
{
    // Values from the published update feed for the 8.5.1 Windows exe
    private static readonly byte[] RealSha256 =
        Convert.FromBase64String("qAXlGbYyd0IClwRqyY8v5PUOQzaKdL9HejEy4Ou8wUY=");

    private static readonly byte[] RealSig256 = Convert.FromBase64String(
        "VQnDrFv1JVSfyzD0GBkEhm3Wz4hnOhPMz/YwrFlOjI5+3qLwXURg3lM7mIC24/avPs1X4aPK7kp5pZv9zfjjW14/+/eZTjHcIH7tEgObAPty" +
        "H6MxiFdwgtoQ+UFV+uFTYLnfuqGwlL9geZmjrjkw122FapnlPCZ+qgg5aw+7OXeHSsO0A1Jjfz8bR91u4hZZAiZTuZJnGV3OHEQNaWi4GdOM" +
        "b0pFwfKCBKv7J6pclp1OyQkY+ftXOX8WF3RaZ6m2zkuazadNN3IejlbS5M6KIWRIb3VRRfvgbid5OerasOKBc7WgJf6TB5XNCSHrUHPBC9Wn" +
        "zIEt0R3UTNmLLEzwgQ==");

    [Fact]
    public void VerifySignature_ValidSignature()
    {
        Assert.True(UpdateOperation.VerifySignature(RealSha256, RealSig256));
    }

    [Fact]
    public void VerifySignature_TamperedHash()
    {
        var sha256 = RealSha256.ToArray();
        sha256[0] ^= 1;

        Assert.False(UpdateOperation.VerifySignature(sha256, RealSig256));
    }

    [Fact]
    public void VerifySignature_TamperedSignature()
    {
        var sig256 = RealSig256.ToArray();
        sig256[^1] ^= 1;

        Assert.False(UpdateOperation.VerifySignature(RealSha256, sig256));
    }

    [Fact]
    public void VerifySignature_SignatureFromOtherKey()
    {
        using var rsa = RSA.Create(2048);
        var sig256 = rsa.SignHash(RealSha256, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        Assert.False(UpdateOperation.VerifySignature(RealSha256, sig256));
    }

    [Fact]
    public void VerifySignature_WrongLengthSignature()
    {
        Assert.False(UpdateOperation.VerifySignature(RealSha256, [1, 2, 3]));
        Assert.False(UpdateOperation.VerifySignature(RealSha256, []));
    }

    [Fact]
    public void VerifyHash_Matches()
    {
        var path = P("update.exe");
        var data = "update contents"u8.ToArray();
        File.WriteAllBytes(path, data);

        Assert.True(UpdateOperation.VerifyHash(path, SHA256.HashData(data)));
    }

    [Fact]
    public void VerifyHash_Mismatch()
    {
        var path = P("update.exe");
        File.WriteAllBytes(path, "update contents"u8.ToArray());

        Assert.False(UpdateOperation.VerifyHash(path, SHA256.HashData("other contents"u8.ToArray())));
        Assert.False(UpdateOperation.VerifyHash(path, RealSha256));
    }
}
