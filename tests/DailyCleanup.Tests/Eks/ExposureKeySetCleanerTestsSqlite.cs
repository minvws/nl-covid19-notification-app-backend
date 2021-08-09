// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Eks
{
    [Trait("db", "mem")]
    public class ExposureKeySetCleanerTestsSqlite : ExposureKeySetCleanerTests, IDisposable
    {
        private static DbConnection connection;
        public ExposureKeySetCleanerTestsSqlite() : base(
            new DbContextOptionsBuilder<ContentDbContext>().UseSqlite(CreateInMemoryDatabase()).Options
        )
        { }

        private static DbConnection CreateInMemoryDatabase()
        {
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
