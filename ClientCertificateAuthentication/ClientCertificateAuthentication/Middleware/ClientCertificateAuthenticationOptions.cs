using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace ClientCertificateAuthentication.Middleware
{
    public class ClientCertificateAuthenticationOptions : AuthenticationOptions
    {
        public ClientCertificateAuthenticationOptions()
        {
            AuthenticationScheme = "Certificate";
            AutomaticAuthenticate = true;
            AutomaticChallenge = true;
        }

        public IEnumerable<TrustedIssuer> TrustedIssuers { get; set; }
    }
}
