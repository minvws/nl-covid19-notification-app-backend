// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Manifest
{
    [Trait("db", "postgres")]
    public class RemoveExpiredManifestsTestsPostgres : RemoveExpiredManifestsTest, IDisposable
    {
        private const string Prefix = nameof(RemoveExpiredManifestsTest) + "_";
        private static DbConnection connection;

        public RemoveExpiredManifestsTestsPostgres() : base(
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
