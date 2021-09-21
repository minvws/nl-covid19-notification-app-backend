// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddTransient(x => new HttpClientHandler());
        }
    }
}
