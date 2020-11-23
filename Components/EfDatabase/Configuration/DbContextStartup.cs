// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public static class DbContextStartup
    {
        public static WorkflowDbContext Workflow(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Workflow);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new WorkflowDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static DkSourceDbContext DkSource(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.DkSource);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new DkSourceDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static EksPublishingJobDbContext EksPublishing(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.EksPublishing);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new EksPublishingJobDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static ContentDbContext Content(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Content);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new ContentDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }
        public static StatsDbContext Stats(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.Stats);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new StatsDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static DataProtectionKeysDbContext DataProtectionKeys(IServiceProvider x)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.DataProtectionKeys);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            return new DataProtectionKeysDbContext(builder.Build());
        }

        public static IksPublishingJobDbContext IksPublishing(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.IksPublishing);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new IksPublishingJobDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static IksInDbContext IksIn(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.IksIn);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new IksInDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

        public static IksOutDbContext IksOut(IServiceProvider x, bool beginTrans = true)
        {
            var config = new StandardEfDbConfig(x.GetRequiredService<IConfiguration>(), DatabaseConnectionStringNames.IksOut);
            var builder = new SqlServerDbContextOptionsBuilder(config, x.GetRequiredService<ILoggerFactory>());
            var result = new IksOutDbContext(builder.Build());
            if (beginTrans) result.BeginTransaction();
            return result;
        }

    }
}