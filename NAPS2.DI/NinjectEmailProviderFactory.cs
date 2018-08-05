using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Imap;
using NAPS2.ImportExport.Email.Mapi;
using Ninject;

namespace NAPS2.DI
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
                default:
                    return kernel.Get<MapiEmailProvider>();
            }
        }
    }
}
