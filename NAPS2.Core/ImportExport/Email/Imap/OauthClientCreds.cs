namespace NAPS2.ImportExport.Email.Imap
{
    internal class OauthClientCreds
    {
        public OauthClientCreds(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string ClientId { get; }

        public string ClientSecret { get; }
    }
}
