using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace NAPS2.Util
{
    public class ServiceFactoryBehavior : IServiceBehavior
    {
        private readonly Func<object> factory;

        public ServiceFactoryBehavior(Func<object> factory)
        {
            this.factory = factory;
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var cdb in serviceHostBase.ChannelDispatchers)
            {
                if (cdb is ChannelDispatcher cd)
                {
                    foreach (var ed in cd.Endpoints)
                    {
                        ed.DispatchRuntime.InstanceProvider = new InstanceProvider(factory);
                    }
                }
            }
        }

        private class InstanceProvider : IInstanceProvider
        {
            private readonly Func<object> factory;

            public InstanceProvider(Func<object> factory)
            {
                this.factory = factory;
            }

            public object GetInstance(InstanceContext instanceContext) => GetInstance(instanceContext, null);

            public object GetInstance(InstanceContext instanceContext, Message message) => factory();

            public void ReleaseInstance(InstanceContext instanceContext, object instance)
            {
            }
        }
    }
}
