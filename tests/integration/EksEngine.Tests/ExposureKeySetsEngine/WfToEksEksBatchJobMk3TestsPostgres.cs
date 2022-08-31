// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "postgres")]
    public class WfToEksEksBatchJobMk3TestsPostgres : WfToEksEksBatchJobMk3Tests, IDisposable
    {
        private const string Prefix = nameof(WfToEksEksBatchJobMk3Tests) + "_";
        private static DbConnection connection;

        public WfToEksEksBatchJobMk3TestsPostgres() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseNpgsql(CreateSqlDatabase("w"))
                .UseSnakeCaseNamingConvention()
                .Options,
            new DbContextOptionsBuilder<DiagnosisKeysDbContext>()
                .UseNpgsql(CreateSqlDatabase("d"))
                .UseSnakeCaseNamingConvention()
                .Options,
            new DbContextOptionsBuilder<EksPublishingJobDbContext>()
                .UseNpgsql(CreateSqlDatabase("p"))
                .UseSnakeCaseNamingConvention()
                .Options,
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
