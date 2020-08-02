// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public static class DbContextStartup
    {
        public static WorkflowDbContext Workflow(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Workflow);
            var builder = new SqlServerDbContextOptionsBuilder(config);
            var result = new WorkflowDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }
        public static PublishingJobDbContext Publishing(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Publishing);
            var builder = new SqlServerDbContextOptionsBuilder(config);
            var result = new PublishingJobDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }
        public static ContentDbContext Content(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Content);
            var builder = new SqlServerDbContextOptionsBuilder(config);
            var result = new ContentDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }
    }
}