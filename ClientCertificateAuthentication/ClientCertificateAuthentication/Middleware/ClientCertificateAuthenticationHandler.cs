using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClientCertificateAuthentication.Middleware
{
    public class ClientCertificateAuthenticationHandler : AuthenticationHandler<ClientCertificateAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public ClientCertificateAuthenticationHandler(ILoggerFactory loggerFactory)
        {
            _logger = (loggerFactory != null) ?
                loggerFactory.CreateLogger<ClientCertificateAuthenticationHandler>() :
                throw new ArgumentNullException(nameof(loggerFactory));
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            _logger.LogInformation("HandleAuthenticateAsync() called.");
            var cert = Context.Connection.ClientCertificate;
            if (cert != null)
            {
                const string failureMessage = "Invalid certificate";

                if (cert.NotBefore > DateTime.Now || cert.NotAfter < DateTime.Now)
                {
                    // Certificate expired or not yet valid
                    _logger.LogDebug(
                        "Certificate with thumbprint '{0}' issued by '{1}' has invalid validity date: {2} - {3}.",
                        cert.Thumbprint,
                        cert.IssuerName.Name,
                        cert.NotBefore,
                        cert.NotAfter);
                    return Task.FromResult(AuthenticateResult.Fail(failureMessage));
                }

                var issuer = Options.TrustedIssuers.SingleOrDefault(x => x.IssuerName == cert.IssuerName.Name);
                if (issuer == null)
                {
                    // Certificate has been issued by an unknown issuer
                    _logger.LogDebug(
                        "Certificate withthumbprint '{0}' issued by '{1}' has been issued by an unknown issuer.",
                        cert.Thumbprint,
                        cert.IssuerName.Name);
                    return Task.FromResult(AuthenticateResult.Fail(failureMessage));
                }

                var subjects = issuer.Subjects.Where(x => x.Name == cert.SubjectName.Name);
                if (subjects.Count() == 0)
                {
                    // Certificate has been issued to an unknown subject
                    _logger.LogDebug(
                        "Certificate withthumbprint '{0}' issued by '{1}' has been issued to an unknown subject '{2}'.",
                        cert.Thumbprint,
                        cert.IssuerName.Name,
                        cert.SubjectName.Name);
                    return Task.FromResult(AuthenticateResult.Fail(failureMessage));
                }

                var subject = subjects.SingleOrDefault(x => x.Thumbprint == cert.Thumbprint);
                if (subject == null)
                {
                    // Certificate thumprint does not match
                    _logger.LogDebug(
                        "Certificate with thumbprint '{0}' issued by '{1}' has an unexpected thumbprint.",
                        cert.Thumbprint,
                        cert.IssuerName.Name);
                    return Task.FromResult(AuthenticateResult.Fail(failureMessage));
                }

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.X500DistinguishedName, cert.SubjectName.Name)
                };
                var identity = new ClaimsIdentity(claims, Options.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme);

                _logger.LogDebug("Certificate with thumbprint '{0}' issued by '{1}' for '{2}' has been authenticated.",
                        cert.Thumbprint,
                        cert.IssuerName.Name,
                        cert.SubjectName.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            _logger.LogInformation("HandleAuthenticateAsync() completed, authentication skipped due to missing certificate.");
            return Task.FromResult(AuthenticateResult.Skip());
        }
    }
}
