using NAPS2.ImportExport.Email.Oauth;

namespace NAPS2.ImportExport.Email;

public class EmailSetup
{
    public EmailProviderType? ProviderType { get; set; }

    public string? SystemProviderName { get; set; }

    public string? GmailUser { get; set; }

    public OauthToken? GmailToken { get; set; }

    public string? OutlookWebUser { get; set; }

    public OauthToken? OutlookWebToken { get; set; }

    public string? SmtpHost { get; set; }

    public string? SmtpFrom { get; set; }

    public int? SmtpPort { get; set; }

    public bool? SmtpTls { get; set; }

    public string? SmtpUser { get; set; }

    public string? SmtpPassword { get; set; }
}