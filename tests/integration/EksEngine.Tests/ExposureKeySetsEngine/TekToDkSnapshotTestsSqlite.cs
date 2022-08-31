// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "mem")]
    public class TekToDkSnapshotTestsSqlite : TekToDkSnapshotTests
    {
        private static DbConnection _connection;

        public TekToDkSnapshotTestsSqlite() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseSqlite(CreateInMemoryDatabase()).Options,
            new DbContextOptionsBuilder<DiagnosisKeysDbContext>().UseSqlite(CreateInMemoryDatabase()).Options
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
