// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Tests
{
    [Trait("db", "mem")]
    [Collection(nameof(ManifestUpdateCommandTestSqlite))]
    [ExclusivelyUses(nameof(ManifestUpdateCommandTestSqlite))]
    public class ManifestUpdateCommandTestSqlite : ManifestUpdateCommandTest, IDisposable
    {
        private static DbConnection _connection;
        public ManifestUpdateCommandTestSqlite() : base(
            new DbContextOptionsBuilder<ContentDbContext>().UseSqlite(CreateInMemoryDatabase()).Options
        )
        { }

        private static DbConnection CreateInMemoryDatabase()
        {
            _connection = new SqliteConnection("Filename=:memory:");

            _connection.Open();

            return _connection;
        }

        public void Dispose() => _connection.Dispose();
    }
}
