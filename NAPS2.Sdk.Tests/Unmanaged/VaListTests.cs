using System.Runtime.InteropServices;
using System.Text;
using NAPS2.Unmanaged;
using Xunit;

namespace NAPS2.Sdk.Tests.Unmanaged;

public class VaListTests
{
    [Fact]
    public void MixedArgs()
    {
        var str = Marshal.StringToHGlobalAnsi("str");
        try
        {
            short s = -3;
            float f = 2.5f;
            Assert.Equal("1 -2 -3 4294967295 9223372036854775807 2.50 -0.125 str",
                Format("%d %d %hd %u %lld %.2f %g %s", 1, -2, s, -1, long.MaxValue, f, -0.125, str));
        }
        finally
        {
            Marshal.FreeHGlobal(str);
        }
    }

    [Fact]
    public void ManyArgs()
    {
        Assert.Equal("1 1.5 2 2.5 3 3.5 4 4.5 5 5.5 6 6.5 7 7.5 8 8.5 9 9.5 10 10.5",
            Format("%d %g %d %g %d %g %d %g %d %g %d %g %d %g %d %g %d %g %d %g",
                1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5));
    }

    [Fact]
    public void NoArgs()
    {
        Assert.Equal("none", Format("none"));
    }

    private static string Format(string format, params VaArg[] args)
    {
        var buffer = new byte[256];
        int length = VaList.Call(ap => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? _vsnprintf(buffer, buffer.Length, format, ap)
            : vsnprintf(buffer, buffer.Length, format, ap), args);
        return Encoding.ASCII.GetString(buffer, 0, length);
    }

    [DllImport("libc", CharSet = CharSet.Ansi)]
    private static extern int vsnprintf(byte[] buffer, nint size, string format, IntPtr ap);

    [DllImport("msvcrt", CharSet = CharSet.Ansi)]
    private static extern int _vsnprintf(byte[] buffer, nint size, string format, IntPtr ap);
}
