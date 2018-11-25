using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Scan.Exceptions;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public class ScanDriverExceptionDetail
    {
        public ScanDriverExceptionDetail()
        {
        }

        public ScanDriverExceptionDetail(ScanDriverException e)
        {
            var stream = new MemoryStream();
            new NetDataContractSerializer().Serialize(stream, e);
            SerializedException = stream.ToArray();
        }

        public byte[] SerializedException { get; set; }

        public ScanDriverException Exception => (ScanDriverException)new NetDataContractSerializer().Deserialize(new MemoryStream(SerializedException));

        public void Throw()
        {
            var exception = Exception;
            exception.PreserveStackTrace();
            throw exception;
        }
    }
}
