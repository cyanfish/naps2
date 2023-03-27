using Autofac;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;

namespace NAPS2;

public class AutofacEmailProviderFactory : IEmailProviderFactory
{
    private readonly IComponentContext _container;

    public AutofacEmailProviderFactory(IComponentContext container)
    {
        _container = container;
    }

    public IEmailProvider Create(EmailProviderType type)
    {
        switch (type)
        {
            case EmailProviderType.Gmail:
                return _container.Resolve<GmailEmailProvider>();
            case EmailProviderType.OutlookWeb:
                return _container.Resolve<OutlookWebEmailProvider>();
            case EmailProviderType.Thunderbird:
                return _container.Resolve<ThunderbirdEmailProvider>();
            case EmailProviderType.AppleMail:
                return _container.Resolve<IAppleMailEmailProvider>();
            default:
                return _container.Resolve<MapiEmailProvider>();
        }
    }

    public IEmailProvider Default
    {
        get
        {
            var config = _container.Resolve<Naps2Config>();
            var providerType = config.Get(c => c.EmailSetup.ProviderType);
            return Create(providerType);
        }
    }
}