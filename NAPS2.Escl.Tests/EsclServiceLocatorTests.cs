using System.Net;
using System.Reflection;
using Makaretu.Dns;
using NAPS2.Escl.Client;
using Xunit;

namespace NAPS2.Escl.Tests;

public class EsclServiceLocatorTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void ServiceRecordsBelongToDiscoveredScanner(bool ipv6, bool addressesFirst, bool addressesInAnswers)
    {
        var instance = "Scanner._uscan._tcp.local";
        var scannerIp = IPAddress.Parse(ipv6 ? "2001:db8::50" : "192.168.100.50");
        var otherIp = IPAddress.Parse(ipv6 ? "2001:db8::99" : "192.168.100.99");
        var message = new Message();
        var addressRecords = addressesInAnswers ? message.Answers : message.AdditionalRecords;
        var addresses = new ResourceRecord[]
        {
            ipv6
                ? new AAAARecord { Name = "SCANNER.local", Address = scannerIp }
                : new ARecord { Name = "SCANNER.local", Address = scannerIp },
            ipv6
                ? new AAAARecord { Name = "other.local", Address = otherIp }
                : new ARecord { Name = "other.local", Address = otherIp }
        };
        if (addressesFirst)
        {
            addressRecords.AddRange(addresses);
        }
        message.Answers.Add(new SRVRecord { Name = instance, Target = "scanner.local", Port = 80 });
        message.Answers.Add(new TXTRecord
        {
            Name = instance,
            Strings = { "uuid=00000000-0000-0000-0000-000000000000", "ty=Scanner", "rs=eSCL" }
        });
        message.Answers.Add(new SRVRecord { Name = "Other._uscan._tcp.local", Target = "other.local", Port = 8080 });
        message.Answers.Add(new TXTRecord
        {
            Name = "Other._uscan._tcp.local",
            Strings = { "uuid=11111111-1111-1111-1111-111111111111", "ty=Other", "rs=wrong" }
        });
        if (!addressesFirst)
        {
            addressRecords.AddRange(addresses);
        }

        var service = ParseService(instance, message);
        Assert.Equal("scanner.local", service.Host);
        Assert.Equal(scannerIp, ipv6 ? service.IpV6 : service.IpV4);
        Assert.Equal(80, service.Port);
        Assert.Equal("Scanner", service.ScannerName);
        Assert.Equal("eSCL", service.RootUrl);
        Assert.Equal("00000000-0000-0000-0000-000000000000", service.Uuid);
        var endpoint = new IPEndPoint(scannerIp, 80).ToString();
#if NET6_0_OR_GREATER
        if (OperatingSystem.IsMacOS())
        {
            endpoint = "scanner.local:80";
        }
#endif
        Assert.Equal($"http://{endpoint}/eSCL", new EsclClient(service).ConnectionUri);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TlsServiceKeepsItsOwnHostAddress(bool tlsFirst)
    {
        var instance = "Scanner._uscan._tcp.local";
        var message = new Message();
        var http = new SRVRecord { Name = instance, Target = "scanner.local", Port = 80 };
        var https = new SRVRecord { Name = "Scanner._uscans._tcp.local", Target = "secure.local", Port = 443 };
        message.Answers.Add(tlsFirst ? https : http);
        message.Answers.Add(tlsFirst ? http : https);
        message.Answers.Add(new TXTRecord
        {
            Name = instance,
            Strings = { "uuid=00000000-0000-0000-0000-000000000000", "ty=Scanner", "rs=eSCL" }
        });
        message.AdditionalRecords.Add(new ARecord { Name = "secure.local", Address = IPAddress.Parse("192.168.100.51") });
        message.AdditionalRecords.Add(new ARecord { Name = "scanner.local", Address = IPAddress.Parse("192.168.100.50") });

        var service = ParseService(instance, message);
        Assert.True(service.Tls);
        Assert.Equal("secure.local", service.Host);
        Assert.Equal(IPAddress.Parse("192.168.100.51"), service.IpV4);
        Assert.Equal(80, service.Port);
        Assert.Equal(443, service.TlsPort);
    }

    private static EsclService ParseService(string instance, Message message)
    {
        using var locator = new EsclServiceLocator(_ => { });
        var parse = typeof(EsclServiceLocator).GetMethod("ParseService", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (EsclService)parse.Invoke(locator, new object[]
        {
            new ServiceInstanceDiscoveryEventArgs
            {
                ServiceInstanceName = instance,
                Message = message,
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.200.1"), 5353)
            }
        })!;
    }
}
