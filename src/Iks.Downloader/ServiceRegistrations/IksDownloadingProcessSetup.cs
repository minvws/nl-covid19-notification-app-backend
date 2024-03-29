// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsDownloader.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsDownloader.ServiceRegistrations
{
    public static class IksDownloadingProcessSetup
    {
        public static void IksDownloadingProcessRegistration(this IServiceCollection services)
        {
            services.AddTransient<IHttpGetIksCommand, HttpGetIksCommand>();
            services.AddTransient<IIksWriterCommand, IksWriterCommand>();
            services.AddTransient<IksPollingBatchJob>();
            services.AddHttpClient(UniversalConstants.EfgsDownloader)
                .ConfigurePrimaryHttpMessageHandler(ConfigureClientCertificate);
        }

        private static HttpMessageHandler ConfigureClientCertificate(IServiceProvider services)
        {
            var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };

            var clientCertificateProvider = services.GetRequiredService<IAuthenticationCertificateProvider>();

            handler.ClientCertificates.Clear();
            handler.ClientCertificates.Add(clientCertificateProvider.GetCertificate());

            return handler;
        }
    }
}
