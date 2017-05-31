using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace TestClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: TestClient <thumbprint>");
                Console.WriteLine("The certificate is loaded by its thumbprint from the current user's certificate store.");
                Environment.Exit(1);
            }

            string thumbprint = args[0];
            var certificate = GetCertificateByThumbprint(thumbprint);
            if (certificate == null)
            {
                Console.WriteLine($"Certificate with thumbprint '{thumbprint}' not found in certificate store.");
                Environment.Exit(2);
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            handler.ClientCertificates.Add(certificate);

            Console.WriteLine("Press any key to send a request.");
            Console.ReadKey(true);

            var client = new HttpClient(handler);
            try
            {
                var response = client.GetAsync("https://localhost:10443/api/values").Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Success.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                client.Dispose();
            }
#if DEBUG
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey(true);
#endif
        }

        private static X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            var cert = default(X509Certificate2);
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certCollection.Count > 0)
                {
                    cert = certCollection[0];
                }
            }
            return cert;
        }
    }
}