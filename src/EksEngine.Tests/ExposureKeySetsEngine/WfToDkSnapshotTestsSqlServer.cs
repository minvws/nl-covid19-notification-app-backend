// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "ss")]
    public class WfToDkSnapshotTestsSqlServer : WfToDkSnapshotTests
    {
        private const string Prefix = nameof(WfToDkSnapshotTests) + "_";
        private static DbConnection _connection;

        public WfToDkSnapshotTestsSqlServer() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseSqlServer(CreateSqlDatabase("W")).Options,
            new DbContextOptionsBuilder<DkSourceDbContext>().UseSqlServer(CreateSqlDatabase("D")).Options,
            new SqlServerWrappedEfExtensions()
        )
        { }

        private static DbConnection CreateSqlDatabase(string suffix)
        {
            var csb = new SqlConnectionStringBuilder($"Data Source=.;Initial Catalog={Prefix + suffix};Integrated Security=True")
            {
                MultipleActiveResultSets = true
            };

            _connection = new SqlConnection(csb.ConnectionString);
            return _connection;
        }

        public void Dispose() => _connection.Dispose();
    }
}
