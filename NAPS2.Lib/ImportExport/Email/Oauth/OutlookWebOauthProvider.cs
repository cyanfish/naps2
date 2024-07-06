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

    protected override string Scope => "mail.readwrite mail.send user.read offline_access";

    protected override string CodeEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

    protected override string TokenEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/token";

    protected override void SaveToken(OauthToken token, bool refresh)
    {
        if (refresh)
        {
            _config.User.Set(c => c.EmailSetup.OutlookWebToken, token);
        }
        else
        {
            var transact = _config.User.BeginTransaction();
            transact.Remove(c => c.EmailSetup);
            transact.Set(c => c.EmailSetup.OutlookWebToken, token);
            transact.Set(c => c.EmailSetup.ProviderType, EmailProviderType.OutlookWeb);
            transact.Commit();
            // We need to commit the token before we can read the email address
            _config.User.Set(c => c.EmailSetup.OutlookWebUser, GetEmailAddress());
        }
    }

    #endregion

    #region Api Methods

    public string GetEmailAddress()
    {
        var resp = GetAuthorized("https://graph.microsoft.com/v1.0/me");
        return resp.Value<string>("mail") ?? throw new InvalidOperationException("Could not get Id from Outlook profile response");
    }

    public async Task<DraftInfo> UploadDraft(string messageRaw, ProgressHandler progress = default)
    {
        var resp = await PostAuthorized("https://graph.microsoft.com/v1.0/me/messages", messageRaw, "application/json", progress);
        var webLink = resp.Value<string>("webLink") ?? throw new InvalidOperationException("Could not get WebLink from Outlook messages response");
        var messageId = resp.Value<string>("id") ?? throw new InvalidOperationException("Could not get ID from Outlook messages response");
        return new DraftInfo(webLink, messageId);
    }

    public async Task SendDraft(string draftMessageId)
    {
        await PostAuthorizedNoResponse($"https://graph.microsoft.com/v1.0/me/messages/{draftMessageId}/send");
    }

    public record DraftInfo(string WebLink, string MessageId);

    #endregion
}