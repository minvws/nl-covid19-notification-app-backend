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
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Trait("db", "ss")]
    public class IksEngineTestsSqlserver : IksEngineTest, IDisposable
    {
        private const string Prefix = nameof(IksEngineTest) + "_";
        private static DbConnection connection;

        public IksEngineTestsSqlserver() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseNpgsql(CreateSqlDatabase("W")).Options,
            new DbContextOptionsBuilder<IksInDbContext>().UseNpgsql(CreateSqlDatabase("II")).Options,
            new DbContextOptionsBuilder<DkSourceDbContext>().UseNpgsql(CreateSqlDatabase("D")).Options,
            new DbContextOptionsBuilder<IksPublishingJobDbContext>().UseNpgsql(CreateSqlDatabase("P")).Options,
            new DbContextOptionsBuilder<IksOutDbContext>().UseNpgsql(CreateSqlDatabase("IO")).Options
        )
        { }

        private static DbConnection CreateSqlDatabase(string suffix)
        {
            var csb = new SqlConnectionStringBuilder($"Data Source=.;Initial Catalog={Prefix + suffix};Integrated Security=True")
            {
                MultipleActiveResultSets = true
            };

            connection = new SqlConnection(csb.ConnectionString);
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
