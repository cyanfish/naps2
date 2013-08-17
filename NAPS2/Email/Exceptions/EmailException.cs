using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Lang.Resources;

namespace NAPS2.Email.Exceptions
{
    public class EmailException : Exception
    {
        public EmailException()
            : base(MiscResources.EmailError)
        {
        }

        public EmailException(string message)
            : base(message)
        {
        }

        public EmailException(Exception innerException)
            : base(MiscResources.EmailError, innerException)
        {
        }

        public EmailException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
