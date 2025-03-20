using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NAPS2.Tools.Project.Targets;
using Newtonsoft.Json;

namespace NAPS2.Tools.Project.Releasing;

public class WebsiteUpdateCommand : ICommand<WebsiteUpdateOptions>
{
    private const string UPDATE_RELEASES_URL = "https://www.naps2.com/hooks/update-releases";
    private const string ADD_FILE_SIG_URL = "https://www.naps2.com/hooks/add-file-sig";

    public int Run(WebsiteUpdateOptions opts)
    {
        var key = File.ReadAllText(Path.Combine(Paths.Naps2UserFolder, "website"));

        var client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "Authorization", key }
            }
        };

        if (!opts.NoSign)
        {
            var certPath = N2Config.AutoUpdateCert;
            if (certPath == null)
            {
                Output.Info("Skipping file signatures as no certificate is configured");
            }
            else
            {
                Output.Info($"Uploading file signatures");

                Console.WriteLine("Password for auto update certificate:");
                var password = Console.ReadLine()?.Trim() ??
                               throw new InvalidOperationException("Password not provided");

                var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, password);
                var sha1 = SHA1.Create();
                var sha256 = SHA256.Create();

                // TODO: All files? These are the only ones we need for auto update
                var exePath = ProjectHelper.GetPackagePath("exe", Platform.Win64, opts.Version);
                var zipPath = ProjectHelper.GetPackagePath("zip", Platform.Win64, opts.Version);

                foreach (var path in new[] { exePath, zipPath })
                {
                    var bytes = File.ReadAllBytes(path);
                    var sha1Hash = sha1.ComputeHash(bytes);
                    var sha256Hash = sha256.ComputeHash(bytes);
                    var sig = cert.GetRSAPrivateKey()!.SignHash(sha1Hash, HashAlgorithmName.SHA1,
                        RSASignaturePadding.Pkcs1);
                    var sig256 = cert.GetRSAPrivateKey()!.SignHash(sha256Hash, HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);
                    File.WriteAllBytes($"{path}.sig", sig);
                    var data = JsonConvert.SerializeObject(new
                    {
                        name = Path.GetFileName(path),
                        sha1 = Convert.ToBase64String(sha1Hash),
                        sha256 = Convert.ToBase64String(sha256Hash),
                        sig = Convert.ToBase64String(sig),
                        sig256 = Convert.ToBase64String(sig256)
                    });
                    var response = client.PostAsync(ADD_FILE_SIG_URL, new StringContent(data, Encoding.UTF8, "application/json")).Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(
                            $"HTTP {response.StatusCode}: {response.Content.ReadAsStringAsync().Result}");
                    }
                }
            }
        }

        if (!opts.NoRelease)
        {
            Output.Info($"Updating releases");
            var response = client.PostAsync(UPDATE_RELEASES_URL, new StringContent("{}", new MediaTypeHeaderValue("application/json")))
                .Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(
                    $"HTTP {response.StatusCode}: {response.Content.ReadAsStringAsync().Result}");
            }
        }

        Output.OperationEnd("Website updated.");
        return 0;
    }
}