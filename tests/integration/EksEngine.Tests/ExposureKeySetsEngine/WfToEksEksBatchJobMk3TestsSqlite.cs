// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "mem")]
    public class WfToEksEksBatchJobMk3TestsSqlite : WfToEksEksBatchJobMk3Tests, IDisposable
    {
        private static DbConnection connection;

        public WfToEksEksBatchJobMk3TestsSqlite() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseSqlite(CreateInMemoryDatabase()).Options,
            new DbContextOptionsBuilder<DiagnosisKeysDbContext>().UseSqlite(CreateInMemoryDatabase()).Options,
            new DbContextOptionsBuilder<EksPublishingJobDbContext>().UseSqlite(CreateInMemoryDatabase()).Options,
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
