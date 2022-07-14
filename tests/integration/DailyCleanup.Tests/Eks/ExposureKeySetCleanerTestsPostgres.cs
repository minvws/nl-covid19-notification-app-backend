// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Eks
{
    [Trait("db", "postgres")]
    public class ExposureKeySetCleanerTestsPostgres : ExposureKeySetCleanerTests
    {
        private const string Prefix = nameof(ExposureKeySetCleanerTests) + "_";
        private static DbConnection connection;

        public ExposureKeySetCleanerTestsPostgres() : base(
            new DbContextOptionsBuilder<ContentDbContext>()
                .UseNpgsql(CreateSqlDatabase("c"))
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
