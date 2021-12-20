// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests
{
    [Trait("db", "ss")]
    [Collection(nameof(EksEngineTestsSqlServer))]
    public class EksEngineTestsSqlServer : EksEngineTests, IDisposable
    {
        private const string Prefix = nameof(EksEngineTests) + "_";
        private static DbConnection connection;

        public EksEngineTestsSqlServer()
            : base(
                new DbContextOptionsBuilder<WorkflowDbContext>().UseSqlServer(CreateSqlDatabase("W")).Options,
                new DbContextOptionsBuilder<DkSourceDbContext>().UseSqlServer(CreateSqlDatabase("D")).Options,
                new DbContextOptionsBuilder<EksPublishingJobDbContext>().UseSqlServer(CreateSqlDatabase("P")).Options,
                new DbContextOptionsBuilder<ContentDbContext>().UseSqlServer(CreateSqlDatabase("C")).Options
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
