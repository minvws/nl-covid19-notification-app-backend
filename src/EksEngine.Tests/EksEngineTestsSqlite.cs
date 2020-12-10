// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests
{
    [Trait("db", "mem")]
    public class EksEngineTestsSqlite : EksEngineTests
    {
        public EksEngineTestsSqlite()
            : base(
                new SqliteInMemoryDbProvider<WorkflowDbContext>(),
                new SqliteInMemoryDbProvider<DkSourceDbContext>(),
                new SqliteInMemoryDbProvider<EksPublishingJobDbContext>(),
                new SqliteInMemoryDbProvider<ContentDbContext>(),
                new SqliteWrappedEfExtensions()
            )
        { }
    }
}
