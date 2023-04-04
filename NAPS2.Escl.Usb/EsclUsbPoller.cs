using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;

namespace NAPS2.Escl.Usb;

public class EsclUsbPoller
{
    private const ClassCode PRINTER_CLASS = ClassCode.Printer;
    private const byte PRINTER_SUBCLASS = 0x01;
    private const byte PRINTER_IPP_USB = 0x04;

    internal static bool IsIppInterface(UsbInterfaceInfo interfaceInfo) =>
        interfaceInfo is
        {
            Class: PRINTER_CLASS,
            SubClass: PRINTER_SUBCLASS,
            Protocol: PRINTER_IPP_USB
        };

    public Task<List<EsclUsbDescriptor>> Poll()
    {
        return Task.Run(() =>
        {
            var result = new List<EsclUsbDescriptor>();
            using var ctx = new UsbContext();
            foreach (var device in ctx.List())
            {
                using var _ = device;
                var interfaces = ((UsbDevice) device).ActiveConfigDescriptor.Interfaces;
                var matchingInterfaces = interfaces.Where(IsIppInterface).ToList();
                if (!matchingInterfaces.Any())
                {
                    continue;
                }
                if (!device.TryOpen())
                {
                    // TODO: Anything to do if we can't open? Retry?
                    continue;
                }
                var info = device.Info;
                // TODO: This doesn't guarantee we have WinUSB installed. We could either try and open the interfaces or find some other way to confirm.
                // Maybe it's worth exposing the driver state to the user so they can differentiate between "can't find device" and "wrong driver" and "broken for some other reason".
                var descriptor = new EsclUsbDescriptor(
                    info.VendorId, info.ProductId, info.SerialNumber, info.Manufacturer, info.Product);
                result.Add(descriptor);
            }
            return result;
        });
    }
}
