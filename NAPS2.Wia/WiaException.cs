using System;
using System.Runtime.Serialization;

namespace NAPS2.Wia;

[Serializable]
public class WiaException : Exception
{
    public static void Check(uint hresult)
    {
        if (hresult != 0)
        {
            throw new WiaException(hresult);
        }
    }

    protected WiaException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public WiaException(uint errorCode) : base($"WIA error code {errorCode:X}")
    {
        ErrorCode = errorCode;
    }

    public uint ErrorCode { get; set; }
}