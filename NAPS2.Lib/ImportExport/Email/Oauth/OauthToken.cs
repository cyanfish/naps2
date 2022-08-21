using NAPS2.Serialization;

namespace NAPS2.ImportExport.Email.Oauth;

public class OauthToken
{
    public SecureString? AccessToken { get; set; }

    public SecureString? RefreshToken { get; set; }

    public DateTime Expiry { get; set; }
}