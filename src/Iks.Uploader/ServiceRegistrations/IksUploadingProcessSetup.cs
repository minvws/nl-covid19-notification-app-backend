// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class IksUploadingProcessSetup
    {
        public static void IksUploadingProcessRegistration(this IServiceCollection services)
        {
            services.AddTransient<IksUploadBatchJob>();

            services.AddTransient<IksSendBatchCommand>();
            services.AddTransient<IBatchTagProvider, BatchTagProvider>();
            services.AddSingleton<HttpPostIksCommand>();
        }
    }
}
