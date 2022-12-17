using NAPS2.Escl.Client;
using Xunit;

namespace NAPS2.Escl.Tests;

public class UsbTests
{
    [Fact]
    public async Task Usb()
    {
        var poller = new EsclUsbPoller();

        var result = await poller.Poll();
        Assert.NotEmpty(result);

        using var usb = new EsclUsbContext(result[0]);
        usb.ConnectToDevice();
        var caps = await usb.Client.GetCapabilities();
        Assert.NotNull(caps);
    }
}