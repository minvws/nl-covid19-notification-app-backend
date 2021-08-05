// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class DbContextSetup
    {
        public static void DbContextRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<IksOutDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut)));
        }
    }
}
