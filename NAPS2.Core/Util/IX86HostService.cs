using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace NAPS2.Util
{
    [ServiceContract]
    public interface IX86HostService
    {
        [OperationContract]
        void DoWork();
    }
}
