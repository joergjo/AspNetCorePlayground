using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ClientCertificateAuthentication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = new WebHostBuilder();
            string environment = webHostBuilder.GetSetting("environment");

            var configBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false)
              .AddJsonFile($"appsettings.{environment}.json", optional: true)
              .AddEnvironmentVariables();

            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                configBuilder.AddUserSecrets<Program>();
            }

            var config = configBuilder.Build();
            string thumbprint = config.GetSection("ServerCertificate").GetValue<string>("Thumbprint");
            var certificate = GetCertificateByThumbprint(thumbprint);

            var host = webHostBuilder
                .UseKestrel(options =>
                {
                    var httpsOptions = new HttpsConnectionFilterOptions
                    {
                        ServerCertificate = certificate,
                        ClientCertificateMode = ClientCertificateMode.RequireCertificate,
#if DEBUG
                        ClientCertificateValidation = (cert, chain, errors) => true,
#endif
                        SslProtocols = SslProtocols.Tls
                    };
                    options.UseHttps(httpsOptions);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls("https://+:10443")
                .Build();

            host.Run();
        }

        private static X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            var cert = default(X509Certificate2);
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];
            }
            return cert;
        }
    }
}
