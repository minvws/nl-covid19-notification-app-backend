// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    public class Sqlite_EksBatchJobMk3Tests : EksBatchJobMk3Tests
    {
        public Sqlite_EksBatchJobMk3Tests() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>(),
            new SqliteInMemoryDbProvider<DkSourceDbContext>(),
            new SqliteInMemoryDbProvider<EksPublishingJobDbContext>(),
            new SqliteInMemoryDbProvider<ContentDbContext>(),
            new SqliteWrappedEfExtensions()
        )
        { }
    }
}