// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class IksUploadingProcessSetup
    {
        private const int RetryCount = 5;

        public static void IksUploadingProcessRegistration(this IServiceCollection services)
        {
            services.AddHttpClient<IksUploadService>()
                .ConfigurePrimaryHttpMessageHandler(
                    x => new EfgsOutboundHttpClientHandler(
                        x.GetRequiredService<IAuthenticationCertificateProvider>(),
                        x.GetRequiredService<ICertificateConfig>()
                    ))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
                .AddPolicyHandler(GetRetryPolicy()) // Retry first
                .AddPolicyHandler(GetCircuitBreakerPolicy()); // After maximum retries, activate CircuitBreaker.

            services.AddTransient<IksUploadBatchJob>();
            services.AddTransient<EfgsOutboundHttpClientHandler>();

            services.AddTransient<IksSendBatchCommand>();
            services.AddTransient<IBatchTagProvider, BatchTagProvider>();
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: RetryCount);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(RetryCount, TimeSpan.FromSeconds(30));
        }
    }
}
