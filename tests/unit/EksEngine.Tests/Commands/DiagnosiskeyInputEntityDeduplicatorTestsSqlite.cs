// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using Xunit;


namespace EksEngine.Tests.Commands
{
    [Trait("db", "mem")]
    public class DiagnosiskeyInputEntityDeduplicatorTestsSqlite : DiagnosiskeyInputEntityDeduplicatorTests, IDisposable
    {
        private static DbConnection connection;

        public DiagnosiskeyInputEntityDeduplicatorTestsSqlite()
            : base(
                new DbContextOptionsBuilder<DkSourceDbContext>().UseSqlite(CreateInMemoryDatabase()).Options
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
