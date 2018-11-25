using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.ImportExport.Email.Mapi;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectEmailProviderFactory : IEmailProviderFactory
    {
        private readonly IKernel kernel;
        private readonly IUserConfigManager userConfigManager;

        public NinjectEmailProviderFactory(IKernel kernel, IUserConfigManager userConfigManager)
        {
            this.kernel = kernel;
            this.userConfigManager = userConfigManager;
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
                var config = userConfigManager.Config;
                var providerType = config.EmailSetup?.ProviderType ?? EmailProviderType.System;
                return Create(providerType);
            }
        }
    }
}
