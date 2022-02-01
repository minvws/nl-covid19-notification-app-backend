// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Collection(nameof(DkToIksSnapshotTestsSqlServer))]
    [Trait("db", "ss")]
    public class DkToIksSnapshotTestsSqlServer : DkToIksSnapshotTests, IDisposable
    {
        private const string Prefix = nameof(DkToIksSnapshotTests) + "_";
        private static DbConnection connection;

        public DkToIksSnapshotTestsSqlServer() : base(
            new DbContextOptionsBuilder<DkSourceDbContext>().UseSqlServer(CreateSqlDatabase("D")).Options,
            new DbContextOptionsBuilder<IksPublishingJobDbContext>().UseSqlServer(CreateSqlDatabase("I")).Options
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