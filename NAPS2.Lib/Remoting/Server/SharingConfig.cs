using System.Collections.Immutable;
using NAPS2.Config.Model;

namespace NAPS2.Remoting.Server;

[Config]
public class SharingConfig
{
    /// <summary>
    /// A unique ID for the NAPS2 instance, so that if you have the same model of scanner connected to different
    /// computers, they still will have unique derived UUIDs.
    /// </summary>
    public Guid? InstanceId { get; set; }

    public ImmutableList<SharedDevice> SharedDevices { get; set; } = [];
}