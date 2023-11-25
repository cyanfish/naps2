using System.Net;
using System.Net.Sockets;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using NAPS2.Escl.Client;

namespace NAPS2.Escl.Usb;

public class EsclUsbContext : IDisposable
{
    private readonly UsbContext _usbContext = new();
    private readonly UsbDevice _device;
    private readonly List<UsbInterfaceInfo> _interfaces;
    private readonly UsbEndpointInfo _bulkOut;
    private readonly UsbEndpointInfo _bulkIn;
    private readonly CancellationTokenSource _cts = new();
    private TcpListener? _proxyListener;
    private EsclClient? _client;

    public EsclUsbContext(EsclUsbDescriptor descriptor)
    {
        _device = FindDevice(descriptor);
        _interfaces = _device.ActiveConfigDescriptor.Interfaces.Where(EsclUsbPoller.IsIppInterface).ToList();
        _bulkOut = _interfaces[0].Endpoints.First(x => x.Attributes == 2 && (x.EndpointAddress & 0x80) == 0);
        _bulkIn = _interfaces[0].Endpoints.First(x => x.Attributes == 2 && (x.EndpointAddress & 0x80) == 0x80);
    }

    public void ConnectToDevice()
    {
        // Claim the port first as we want to fail before opening USB connections if needed
        if (_proxyListener == null)
        {
            _proxyListener = new TcpListener(IPAddress.Loopback, 0);
            _proxyListener.Start();
        }

        _device.Open();
        foreach (var interfaceInfo in _interfaces)
        {
            _device.ClaimInterface(interfaceInfo.Number);
            _device.SetAltInterface(interfaceInfo.AlternateSetting);
        }

        var port = ((IPEndPoint) _proxyListener.LocalEndpoint).Port;
        _client = new EsclClient(new EsclService
        {
            IpV4 = IPAddress.Loopback,
            IpV6 = null,
            Host = IPAddress.Loopback.ToString(),
            RemoteEndpoint = IPAddress.Loopback,
            Port = port,
            RootUrl = "eSCL",
            Tls = false
        });
        Task.Run(ProxyLoop);
    }

    private async Task ProxyLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            // TODO: Verify this works with cancellation (net462 doesn't have an overload that takes a token but we do call Stop so that might do it)
            using var tcpClient = await _proxyListener!.AcceptTcpClientAsync();
            // TODO: Handle exceptions
            using var tcpStream = tcpClient.GetStream();
            var requestStream = CopyFromNetworkStream(tcpStream);
            var requestText = StreamToString(requestStream);
            WriteToDevice(requestStream);
            var responseStream = ReadFromDevice();
            var responseText = StreamToString(responseStream);
            CopyToNetworkStream(tcpStream, responseStream);
        }
    }

    private string StreamToString(MemoryStream stream)
    {
        return Encoding.UTF8.GetString(stream.GetBuffer().Take((int) stream.Length).ToArray());
    }

    private MemoryStream CopyFromNetworkStream(NetworkStream tcpStream)
    {
        var stream = new MemoryStream();
        var buffer = new byte[65_536];
        tcpStream.ReadTimeout = 50;
        try
        {
            while (tcpStream.Read(buffer, 0, buffer.Length) is var bytesRead and > 0)
            {
                stream.Write(buffer, 0, bytesRead);
            }
        }
        catch (Exception)
        {
            if (stream.Length == 0) throw;
        }
        return stream;
    }

    private static void CopyToNetworkStream(NetworkStream tcpStream, MemoryStream stream)
    {
        tcpStream.Write(stream.GetBuffer(), 0, (int) stream.Length);
    }

    private MemoryStream ReadFromDevice()
    {
        var stream = new MemoryStream();
        var buffer = new byte[65_536];
        var reader = _device.OpenEndpointReader((ReadEndpointID) _bulkIn.EndpointAddress, buffer.Length);
        // TODO: Do we need to handle longer timeouts and/or detect the end of stream based on content? e.g. if a chunked response
        while (reader.Read(buffer, 0, buffer.Length, 1000, out var bytesRead) == Error.Success)
        {
            stream.Write(buffer, 0, bytesRead);
        }
        return stream;
    }

    private void WriteToDevice(MemoryStream memoryStream)
    {
        var writer = _device.OpenEndpointWriter((WriteEndpointID) _bulkOut.EndpointAddress);
        // TODO: Do we need to handle incomplete transfers?
        var status = writer.Write(memoryStream.GetBuffer(), 0, (int) memoryStream.Length, 1000, out var _);
        if (status != Error.Success)
        {
            // Do something
        }
    }

    public EsclClient Client => _client ??
                                throw new InvalidOperationException(
                                    "You must call ConnectToDevice before getting the Client.");

    private UsbDevice FindDevice(EsclUsbDescriptor descriptor)
    {
        foreach (var device in _usbContext.List())
        {
            if (device.Info.ProductId == descriptor.ProductId &&
                device.Info.VendorId == descriptor.VendorId)
            {
                // TODO: Also check serial number? Would need to open device
                // && device.Info.SerialNumber == descriptor.SerialNumber
                return (UsbDevice) device;
            }
            device.Dispose();
        }
        throw new Exception($"ESCL USB device not found: {descriptor.Manufacturer} {descriptor.Product}");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _proxyListener?.Stop();
        _device.Dispose();
        _usbContext.Dispose();
    }
}
