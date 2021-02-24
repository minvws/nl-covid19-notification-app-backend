using System;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Extensions
{
    public static class RestApiClientExtensions
    {
        public static IServiceCollection AddRestApiClient(this IServiceCollection services, IccPortalConfig configuration, string userAgent = null)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            
            var userAgentValue = userAgent ?? Assembly.GetCallingAssembly().GetName().Name;

            services.AddHttpClient<IRestApiClient, RestApiClient>(client =>
            {
                client.BaseAddress = new Uri(configuration.BackendBaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", userAgentValue);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }

    public class BaseUrls
    {
        public string IccBackendBaseUrl { get; set; }
    }
}
