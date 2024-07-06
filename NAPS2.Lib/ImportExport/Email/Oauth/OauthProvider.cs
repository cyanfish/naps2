using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NAPS2.Serialization;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth;

// TODO: Migrate to HttpClient
#pragma warning disable SYSLIB0014
public abstract class OauthProvider
{
    private static readonly int[] PortNumbers = { 50086, 53893, 58985, 49319, 50320 };

    public abstract OauthToken? Token { get; }

    public abstract string? User { get; }

    public bool HasClientCreds => ClientCreds.ClientId != null;

    protected abstract OauthClientCreds ClientCreds { get; }

    protected abstract string CodeEndpoint { get; }

    protected abstract string TokenEndpoint { get; }

    protected abstract string Scope { get; }

    public void AcquireToken(CancellationToken cancelToken)
    {
        // Initialize state, port, and redirectUri
        byte[] buffer = new byte[16];
        SecureStorage.CryptoRandom.Value.GetBytes(buffer);
        string state = string.Join("", buffer.Select(b => b.ToString("x")));
        int port = GetUnusedPort();
        var redirectUri = $"http://127.0.0.1:{port}/";

        // Listen on the redirect uri for the code
        var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        // Abort the listener if the user cancels
        cancelToken.Register(() => listener.Abort());
        cancelToken.ThrowIfCancellationRequested();
        // TODO: Catch exception on abort

        // Open the user interface (which will redirect to our localhost listener)
        var url =
            $"{CodeEndpoint}?scope={Scope}&response_type=code&state={state}&redirect_uri={redirectUri}&client_id={ClientCreds.ClientId}";
        ProcessHelper.OpenUrl(url);

        // Wait for the authorization code to be sent to the local socket
        string code;
        while (true)
        {
            var ctx = listener.GetContext();
            var queryString = ctx.Request.QueryString;

            string responseString = "<script>location.href = 'about:blank';</script>";
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
            var response = ctx.Response;
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            response.OutputStream.Close();

            // Validate the state (standard oauth2 security)
            var requestState = queryString.Get("state");
            if (requestState == state)
            {
                // Yay, we got an authorization code
                code = queryString.Get("code") ?? throw new InvalidOperationException();
                break;
            }
        }
        listener.Stop();
        cancelToken.ThrowIfCancellationRequested();

        // Trade the code in for a token
        var resp = Post(TokenEndpoint, new NameValueCollection
        {
            { "code", code },
            { "client_id", ClientCreds.ClientId },
            { "client_secret", ClientCreds.ClientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        });
        SaveToken(new OauthToken
        {
            AccessToken = resp.Value<string>("access_token"),
            RefreshToken = resp.Value<string>("refresh_token"),
            Expiry = DateTime.Now.AddSeconds(resp.Value<int>("expires_in"))
        }, false);
    }

    public void RefreshToken()
    {
        if (Token?.RefreshToken == null) throw new InvalidOperationException();
        var resp = Post(TokenEndpoint, new NameValueCollection
        {
            { "refresh_token", Token.RefreshToken },
            { "client_id", ClientCreds.ClientId },
            { "client_secret", ClientCreds.ClientSecret },
            { "grant_type", "refresh_token" }
        });
        // TODO: Handle failure
        SaveToken(new OauthToken
        {
            AccessToken = resp.Value<string>("access_token"),
            RefreshToken = Token.RefreshToken,
            Expiry = DateTime.Now.AddSeconds(resp.Value<int>("expires_in"))
        }, true);
    }

    private static int GetUnusedPort()
    {
        foreach (var port in PortNumbers)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
            }
        }
        throw new InvalidOperationException("No available port");
    }

    protected abstract void SaveToken(OauthToken token, bool refresh);

    protected JObject Get(string url)
    {
        using var client = new WebClient();
        string response = client.DownloadString(url);
        return JObject.Parse(response);
    }

    protected JObject GetAuthorized(string url)
    {
        using var client = AuthorizedClient();
        string response = client.DownloadString(url);
        return JObject.Parse(response);
    }

    protected JObject Post(string url, NameValueCollection values)
    {
        using var client = new WebClient();
        string response = Encoding.UTF8.GetString(client.UploadValues(url, "POST", values));
        return JObject.Parse(response);
    }

    protected async Task<JObject> PostAuthorized(string url, string body, string contentType,
        ProgressHandler progress = default)
    {
        using var client = AuthorizedClient();
        client.Headers.Add("Content-Type", contentType);
        // TODO: Apparently upload progress doesn't work.
        // It tracks progress to an internal buffer.
        // https://stackoverflow.com/questions/8181114/uploading-http-progress-tracking
        // Maybe using HttpClient would fix it.
        //client.AddUploadProgressHandler(progressCallback);
        string response = await client.UploadStringTaskAsync(url, "POST", body, progress.CancelToken);
        return JObject.Parse(response);
    }

    protected async Task PostAuthorizedNoResponse(string url)
    {
        using var client = AuthorizedClient();
        await client.UploadStringTaskAsync(url, "POST", "");
    }

    private WebClient AuthorizedClient()
    {
        var client = new WebClient();
        var token = Token;
        if (token?.AccessToken != null && !string.IsNullOrEmpty(token.AccessToken))
        {
            if (token.Expiry < DateTime.Now + TimeSpan.FromMinutes(10))
            {
                RefreshToken();
            }
            client.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
        }
        return client;
    }
}