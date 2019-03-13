using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Recovery
{
    public class RecoveryIndexSerializer : VersionedSerializer<RecoveryIndex>
    {
        protected override void InternalSerialize(Stream stream, RecoveryIndex obj) => XmlSerialize(stream, obj);

        protected override RecoveryIndex InternalDeserialize(Stream stream, string rootName, int version) => XmlDeserialize(stream);

        protected override IEnumerable<Type> KnownTypes => Transform.KnownTransformTypes;
    }
}
