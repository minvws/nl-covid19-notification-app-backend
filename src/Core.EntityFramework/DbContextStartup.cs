// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{


    public static class DbContextStartup
    {
        public static T CreateDbContext<T>(this IServiceProvider serviceProvider, Func<DbContextOptions, T> ctor, string connName, bool beginTrans = true) where T : DbContext
        {
            var config = new StandardEfDbConfig(serviceProvider.GetRequiredService<IConfiguration>(), connName);
            var builder = new SqlServerDbContextOptionsBuilder(config, serviceProvider.GetRequiredService<ILoggerFactory>());
            var result = ctor(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }
    }
}