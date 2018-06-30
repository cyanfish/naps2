using System;
using System.Runtime.Serialization;

namespace NAPS2.ImportExport
{
    [Serializable()]
    public class ImageRenderException : Exception
    {
        public ImageRenderException()
        {
        }

        public ImageRenderException(string message) : base(message)
        {
        }

        public ImageRenderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ImageRenderException(SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}