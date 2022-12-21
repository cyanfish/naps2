using System.Text;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth;

// TODO: The config references should be pulled elsewhere so this can be included in the SDK
public class GmailOauthProvider : OauthProvider
{
    private readonly Naps2Config _config;

    private OauthClientCreds? _creds;

    public GmailOauthProvider(Naps2Config config)
    {
        _config = config;
    }

    #region Authorization

    public override OauthToken? Token => _config.Get(c => c.EmailSetup.GmailToken);

    public override string? User => _config.Get(c => c.EmailSetup.GmailUser);

    protected override OauthClientCreds ClientCreds
    {
        get
        {
            if (_creds == null)
            {
                var credObj = JObject.Parse(Encoding.UTF8.GetString(ClientCreds_.google_credentials));
                var installed = credObj.Value<JObject>("installed");
                _creds = new OauthClientCreds(installed?.Value<string>("client_id"),
                    installed?.Value<string>("client_secret"));
            }
            return _creds;
        }
    }

    protected override string Scope => "https://www.googleapis.com/auth/gmail.compose";

    protected override string CodeEndpoint => "https://accounts.google.com/o/oauth2/v2/auth";

    protected override string TokenEndpoint => "https://www.googleapis.com/oauth2/v4/token";

    protected override void SaveToken(OauthToken token, bool refresh)
    {
        var emailSetup = _config.Get(c => c.EmailSetup);
        emailSetup.GmailToken = token;
        if (!refresh)
        {
            emailSetup.GmailUser = GetEmailAddress();
            emailSetup.ProviderType = EmailProviderType.Gmail;
        }
        _config.User.Set(c => c.EmailSetup, emailSetup);
    }

    #endregion

    #region Api Methods

    public string GetEmailAddress()
    {
        var resp = GetAuthorized("https://www.googleapis.com/gmail/v1/users/me/profile");
        return resp.Value<string>("emailAddress") ??
               throw new InvalidOperationException("Could not get emailAddress from Gmail profile response");
    }

    public async Task<DraftInfo> UploadDraft(string messageRaw, ProgressHandler progress = default)
    {
        var resp = await PostAuthorized(
            $"https://www.googleapis.com/upload/gmail/v1/users/{User}/drafts?uploadType=multipart",
            messageRaw,
            "message/rfc822",
            progress);
        return new DraftInfo(
            resp.Value<JObject>("message")?.Value<string>("id") ??
            throw new InvalidOperationException("Could not get message id from Gmail draft upload response"),
            resp.Value<string>("id") ??
            throw new InvalidOperationException("Could not get id from Gmail draft upload response")
        );
    }

    public async Task SendDraft(string draftId)
    {
        await PostAuthorized($"https://www.googleapis.com/gmail/v1/users/{User}/drafts/send",
            new JObject
            {
                { "id", draftId }
            }.ToString(),
            "application/json");
    }

    public record DraftInfo(string DraftId, string MessageId);

    #endregion
}