using System.Text;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth;

public class OutlookWebOauthProvider : OauthProvider
{
    private readonly Naps2Config _config;

    private OauthClientCreds? _creds;

    public OutlookWebOauthProvider(Naps2Config config)
    {
        _config = config;
    }

    #region Authorization

    public override OauthToken? Token => _config.Get(c => c.EmailSetup.OutlookWebToken);

    public override string? User => _config.Get(c => c.EmailSetup.OutlookWebUser);

    protected override OauthClientCreds ClientCreds
    {
        get
        {
            if (_creds == null)
            {
                var credObj = JObject.Parse(Encoding.UTF8.GetString(ClientCreds_.microsoft_credentials));
                _creds = new OauthClientCreds(credObj.Value<string>("client_id"), credObj.Value<string>("client_secret"));
            }
            return _creds;
        }
    }

    protected override string Scope => "https://outlook.office.com/mail.readwrite https://outlook.office.com/mail.send https://outlook.office.com/user.read offline_access";

    protected override string CodeEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

    protected override string TokenEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/token";

    protected override void SaveToken(OauthToken token, bool refresh)
    {
        var emailSetup = _config.Get(c => c.EmailSetup);
        emailSetup.OutlookWebToken = token;
        if (!refresh)
        {
            emailSetup.OutlookWebUser = GetEmailAddress();
            emailSetup.ProviderType = EmailProviderType.OutlookWeb;
        }
        _config.User.Set(c => c.EmailSetup, emailSetup);
    }

    #endregion

    #region Api Methods

    public string GetEmailAddress()
    {
        var resp = GetAuthorized("https://outlook.office.com/api/v1.0/me");
        return resp.Value<string>("Id");
    }

    public async Task<string> UploadDraft(string messageRaw, ProgressHandler progress = default)
    {
        var resp = await PostAuthorized("https://outlook.office.com/api/v1.0/me/messages", messageRaw, "application/json", progress);
        return resp.Value<string>("WebLink");
    }

    #endregion
}