// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands.Tests.Stats
{
    [Trait("db", "postgres")]
    public class StatsTestsPostgres : StatsTests
    {
        private const string Prefix = nameof(StatsTests) + "_";
        private static DbConnection connection;

        public StatsTestsPostgres() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseNpgsql(CreateSqlDatabase("w"))
                .UseSnakeCaseNamingConvention()
                .Options,
            new DbContextOptionsBuilder<StatsDbContext>()
                .UseNpgsql(CreateSqlDatabase("s"))
                .UseSnakeCaseNamingConvention()
                .Options
        )
        { }

        private static DbConnection CreateSqlDatabase(string suffix)
        {
            var csb = new NpgsqlConnectionStringBuilder($"Host=localhost;Database={Prefix + suffix}");

            connection = new NpgsqlConnection(csb.ConnectionString);
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
