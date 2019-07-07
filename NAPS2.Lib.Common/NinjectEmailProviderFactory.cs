using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using Ninject;

namespace NAPS2
{
    public class NinjectEmailProviderFactory : IEmailProviderFactory
    {
        private readonly IKernel kernel;

        public NinjectEmailProviderFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IEmailProvider Create(EmailProviderType type)
        {
            switch (type)
            {
                case EmailProviderType.Gmail:
                    return kernel.Get<GmailEmailProvider>();
                case EmailProviderType.OutlookWeb:
                    return kernel.Get<OutlookWebEmailProvider>();
                default:
                    return kernel.Get<MapiEmailProvider>();
            }
        }

        public IEmailProvider Default
        {
            get
            {
                var config = kernel.Get<ConfigProvider<CommonConfig>>();
                var providerType = config.Get(c => c.EmailSetup.ProviderType);
                return Create(providerType);
            }
        }
    }
}
