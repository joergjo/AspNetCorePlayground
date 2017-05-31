using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Encodings.Web;

namespace ClientCertificateAuthentication.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ClientCertificateAuthenticationMiddleware : AuthenticationMiddleware<ClientCertificateAuthenticationOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public ClientCertificateAuthenticationMiddleware(RequestDelegate next, IOptions<ClientCertificateAuthenticationOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder)
            : base(next, options, loggerFactory, encoder)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override AuthenticationHandler<ClientCertificateAuthenticationOptions> CreateHandler() => 
            new ClientCertificateAuthenticationHandler(_loggerFactory);
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ClientCertificateAuthenticationExtensions
    {
        public static IApplicationBuilder UseClientCertificateAuthentication(this IApplicationBuilder builder) => 
            builder.UseMiddleware<ClientCertificateAuthenticationMiddleware>();

        public static IApplicationBuilder UseClientCertificateAuthentication(this IApplicationBuilder builder, ClientCertificateAuthenticationOptions options) =>
           builder.UseMiddleware<ClientCertificateAuthenticationMiddleware>(options);
    }
}
