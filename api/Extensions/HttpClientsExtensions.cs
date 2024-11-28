using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ocpa.ro.api.Models.Configuration;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ocpa.ro.api.Extensions
{
    public static class HttpClientsExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient("geolocation", (serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<GeoLocationConfig>>().Value;
                var uriBuilder = new UriBuilder(settings.BaseUrl);
                client.BaseAddress = uriBuilder.Uri;

            }).HandleCertificateErrors();

            services.AddHttpClient("caas", (serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<CaasConfig>>().Value;
                var uriBuilder = new UriBuilder(settings.BaseUrl);
                client.BaseAddress = uriBuilder.Uri;
                client.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "no-cache");

            }).HandleCertificateErrors();

            return services;
        }

#pragma warning disable S4830
        private static IHttpClientBuilder HandleCertificateErrors(this IHttpClientBuilder httpClientBuilder)
            => httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,

                // This is meant as a temporary workaround for client certificate errors such as
                // "unable to get local issuer certificate", "untrusted root" and so on, that we
                // encounter when we try accessing resources in the INSIM.BIZ domain.
                ServerCertificateCustomValidationCallback = CustomCertificateCheck

            });

        private static bool CustomCertificateCheck(HttpRequestMessage msg, X509Certificate2 cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            => HttpClientHandler.DangerousAcceptAnyServerCertificateValidator(msg, cert, chain, sslPolicyErrors);
#pragma warning restore S4830
    }
}
