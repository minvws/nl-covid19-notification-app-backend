// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework
{
    public class SqlServerDbProvider<TDbContext> : IDbProvider<TDbContext> where TDbContext : DbContext
    {
        public SqlServerDbProvider(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException(nameof(databaseName));

            var csb = new SqlConnectionStringBuilder($"Data Source=.;Initial Catalog={databaseName};Integrated Security=True")
            {
                MultipleActiveResultSets = true
            };

            var ctor = typeof(TDbContext).GetConstructor(new[] { typeof(DbContextOptions) });

            CreateNew = () =>
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder().UseSqlServer(csb.ConnectionString);
                return (TDbContext)ctor.Invoke(new object[] { dbContextOptionsBuilder.Options });
            };

            CreateNewWithTx = () =>
            {
                var d = CreateNew();
                d.BeginTransaction();
                return d;
            };

            using var dbc = CreateNew();
            dbc.Database.EnsureDeleted();
            dbc.Database.EnsureCreated();
        }

        public Func<TDbContext> CreateNew { get; }

        public Func<TDbContext> CreateNewWithTx { get; }

        public void Dispose()
        {
        }
    }
}