// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Trait("db", "postgres")]
    public class IksEngineTestsPostgres : IksEngineTest, IDisposable
    {
        private const string Prefix = nameof(IksEngineTest) + "_";
        private static DbConnection connection;

        public IksEngineTestsPostgres() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseNpgsql(CreateDatabase("W")).Options,
            new DbContextOptionsBuilder<IksInDbContext>().UseNpgsql(CreateDatabase("II")).Options,
            new DbContextOptionsBuilder<DkSourceDbContext>().UseNpgsql(CreateDatabase("D")).Options,
            new DbContextOptionsBuilder<IksPublishingJobDbContext>().UseNpgsql(CreateDatabase("P")).Options,
            new DbContextOptionsBuilder<IksOutDbContext>().UseNpgsql(CreateDatabase("IO")).Options
        )
        { }

        private static DbConnection CreateDatabase(string suffix)
        {
            var csb = new NpgsqlConnectionStringBuilder($"Host=localhost;Database={Prefix + suffix};");

            connection = new NpgsqlConnection(csb.ConnectionString);
            connection.Open();

            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
