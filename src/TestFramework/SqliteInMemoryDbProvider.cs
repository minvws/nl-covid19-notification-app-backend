// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework
{
    public class SqliteInMemoryDbProvider<TDbContext> : IDbProvider<TDbContext> where TDbContext : DbContext
    {
        private readonly SqliteConnection _Connection = new SqliteConnection("Data Source=:memory:");

        public SqliteInMemoryDbProvider()
        {
            var ctor = typeof(TDbContext).GetConstructor(new[] { typeof(DbContextOptions) });
            CreateNew = () => (TDbContext)ctor.Invoke(new object[] { new DbContextOptionsBuilder().UseSqlite(_Connection).Options });
            CreateNewWithTx = //Cannot new tx?
                () =>
                {
                    var c = CreateNew();
                    c.BeginTransaction();
                    return c;
                };

            var dbc = CreateNew();
            _Connection.Open(); //Database created.
            dbc.Database.EnsureCreated();
        }

        public Func<TDbContext> CreateNew { get; }
        public Func<TDbContext> CreateNewWithTx { get; }

        public void Dispose()
        {
            _Connection?.Dispose();
        }
    }
}