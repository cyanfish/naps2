using NAPS2.Config.Model;
using NAPS2.Escl.Server;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Remoting;

public class SharedDeviceManagerTests : ContextualTests
{
    private readonly SharedDevice _device1 = MakeDevice("1");
    private readonly SharedDevice _device2 = MakeDevice("2");
    private readonly List<IEsclServer> _esclServers = [];
    private readonly List<SharedDeviceManager> _managers = [];

    private static SharedDevice MakeDevice(string id) =>
        new() { Name = $"name{id}", Device = new ScanDevice(Driver.Wia, $"id{id}", $"name{id}") };

    private SharedDeviceManager CreateManager()
    {
        var manager = new SharedDeviceManager(ScanningContext, Naps2Config.Stub(), P("sharing.xml"), P("sharing.lock"),
            () =>
            {
                var esclServer = Substitute.For<IEsclServer>();
                _esclServers.Add(esclServer);
                return esclServer;
            });
        _managers.Add(manager);
        return manager;
    }

    public override ValueTask DisposeAsync()
    {
        foreach (var manager in _managers)
        {
            manager.StopSharing();
        }
        return base.DisposeAsync();
    }

    private static void AssertRegistered(IEsclServer esclServer, params SharedDevice[] devices)
    {
        var registered = esclServer.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IEsclServer.AddDevice))
            .Select(call => ((EsclDeviceConfig) call.GetArguments()[0]!).Capabilities.MakeAndModel);
        Assert.Equal(devices.Select(device => device.Name), registered);
    }

    private static string GetRegisteredUuid(IEsclServer esclServer) =>
        ((EsclDeviceConfig) esclServer.ReceivedCalls()
            .First(call => call.GetMethodInfo().Name == nameof(IEsclServer.AddDevice))
            .GetArguments()[0]!).Capabilities.Uuid!;

    [Fact]
    public void NoServerWithoutSharedDevices()
    {
        var manager = CreateManager();

        manager.StartSharing();
        manager.StopSharing();

        Assert.Empty(_esclServers);
    }

    [Fact]
    public void NoServerForDeviceChangesBeforeStart()
    {
        var manager = CreateManager();

        manager.AddSharedDevice(_device1);
        manager.ReplaceSharedDevice(_device1, _device2);
        manager.RemoveSharedDevice(_device2);

        Assert.Empty(_esclServers);
        Assert.Empty(manager.SharedDevices);
    }

    [Fact]
    public void DevicesAddedBeforeStartAreShared()
    {
        var manager = CreateManager();
        manager.AddSharedDevice(_device1);
        manager.AddSharedDevice(_device2);

        manager.StartSharing();

        var esclServer = Assert.Single(_esclServers);
        AssertRegistered(esclServer, _device1, _device2);
        esclServer.Received(1).Start();
    }

    [Fact]
    public void DeviceAddedAfterDeferredStartIsShared()
    {
        var manager = CreateManager();
        manager.StartSharing();
        Assert.Empty(_esclServers);

        manager.AddSharedDevice(_device1);

        var esclServer = Assert.Single(_esclServers);
        AssertRegistered(esclServer, _device1);
        esclServer.Received(1).Start();
    }

    [Fact]
    public void DeviceChangesUpdateRunningServer()
    {
        var manager = CreateManager();
        manager.AddSharedDevice(_device1);
        manager.StartSharing();
        var esclServer = Assert.Single(_esclServers);

        manager.AddSharedDevice(_device2);
        AssertRegistered(esclServer, _device1, _device2);

        manager.RemoveSharedDevice(_device1);
        esclServer.ReceivedWithAnyArgs(1).RemoveDevice(null!);

        manager.ReplaceSharedDevice(_device2, _device1);
        esclServer.ReceivedWithAnyArgs(2).RemoveDevice(null!);
        AssertRegistered(esclServer, _device1, _device2, _device1);
        Assert.Single(_esclServers);
    }

    [Fact]
    public void StopSharingStopsServer()
    {
        var manager = CreateManager();
        manager.AddSharedDevice(_device1);
        manager.StartSharing();

        manager.StopSharing();

        Assert.Single(_esclServers).Received(1).Stop();
    }

    [Fact]
    public void InstanceIdSavedBeforeServerCreated()
    {
        var manager = CreateManager();
        manager.AddSharedDevice(_device1);
        manager.StartSharing();
        manager.StopSharing();

        // A new instance reads the saved ID, so the device keeps the same UUID
        var manager2 = CreateManager();
        manager2.StartSharing();

        Assert.Equal(2, _esclServers.Count);
        Assert.Equal(GetRegisteredUuid(_esclServers[0]), GetRegisteredUuid(_esclServers[1]));
    }

    [Fact]
    public void InstanceIdSavedAfterServerCreated()
    {
        // Devices but no instance ID in the config, so the server is created with a new ID before one is saved
        var scope = ConfigScope.File(P("sharing.xml"), new ConfigStorageSerializer<SharingConfig>(),
            ConfigScopeMode.ReadWrite);
        scope.Set(c => c.SharedDevices, [_device1]);
        var manager = CreateManager();
        manager.StartSharing();

        manager.AddSharedDevice(_device2);
        manager.StopSharing();
        var manager2 = CreateManager();
        manager2.StartSharing();

        Assert.Equal(2, _esclServers.Count);
        Assert.Equal(GetRegisteredUuid(_esclServers[0]), GetRegisteredUuid(_esclServers[1]));
    }
}
