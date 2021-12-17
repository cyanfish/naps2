using NAPS2.Serialization;

namespace NAPS2.Recovery;

public class RecoveryIndexSerializer : VersionedSerializer<RecoveryIndex>
{
    protected override void InternalSerialize(Stream stream, RecoveryIndex obj) => XmlSerialize(stream, obj);

    protected override RecoveryIndex InternalDeserialize(Stream stream, XDocument doc) => XmlDeserialize(stream);
}