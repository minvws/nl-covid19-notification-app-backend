// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Collection(nameof(MarkDiagnosisKeysAsUsedByIksTestsPostgres))]
    [Trait("db", "postgres")]
    public class MarkDiagnosisKeysAsUsedByIksTestsPostgres : MarkDiagnosisKeysAsUsedByIksTests, IDisposable
    {
        private const string Prefix = nameof(MarkDiagnosisKeysAsUsedByIksTests) + "_";
        private static DbConnection connection;

        public MarkDiagnosisKeysAsUsedByIksTestsPostgres() : base(
            new DbContextOptionsBuilder<DiagnosisKeysDbContext>()
                .UseNpgsql(CreateDatabase("d"))
                .UseSnakeCaseNamingConvention()
                .Options,
            new DbContextOptionsBuilder<IksPublishingJobDbContext>()
                .UseNpgsql(CreateDatabase("ipj"))
                .UseSnakeCaseNamingConvention()
                .Options
        )
        { }

        private static DbConnection CreateDatabase(string suffix)
        {
            var csb = new NpgsqlConnectionStringBuilder($"Host=localhost;Database={Prefix + suffix};");

            connection = new NpgsqlConnection(csb.ConnectionString);

            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
