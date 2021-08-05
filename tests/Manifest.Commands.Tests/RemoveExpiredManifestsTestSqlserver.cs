// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    [Trait("db", "ss")]
    public class RemoveExpiredManifestsTestSqlserver : RemoveExpiredManifestsTest, IDisposable
    {
        private const string Prefix = nameof(RemoveExpiredManifestsTest) + "_";
        private static DbConnection connection;

        public RemoveExpiredManifestsTestSqlserver() : base(
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
