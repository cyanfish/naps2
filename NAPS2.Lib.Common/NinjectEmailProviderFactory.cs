using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using Ninject;

namespace NAPS2;

public class NinjectEmailProviderFactory : IEmailProviderFactory
{
    private readonly IKernel _kernel;

    public NinjectEmailProviderFactory(IKernel kernel)
    {
        _kernel = kernel;
    }

    public IEmailProvider Create(EmailProviderType type)
    {
        switch (type)
        {
            case EmailProviderType.Gmail:
                return _kernel.Get<GmailEmailProvider>();
            case EmailProviderType.OutlookWeb:
                return _kernel.Get<OutlookWebEmailProvider>();
            default:
                return _kernel.Get<MapiEmailProvider>();
        }
    }

    public IEmailProvider Default
    {
        get
        {
            var config = _kernel.Get<Naps2Config>();
            var providerType = config.Get(c => c.EmailSetup.ProviderType);
            return Create(providerType);
        }
    }
}