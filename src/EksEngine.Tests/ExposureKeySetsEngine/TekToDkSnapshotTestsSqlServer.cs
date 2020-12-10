// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "ss")]
    public class TekToDkSnapshotTestsSqlServer : TekToDkSnapshotTests
    {
        private const string Prefix = nameof(TekToDkSnapshotTests) + "_";
        public TekToDkSnapshotTestsSqlServer() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix+"W"),
            new SqlServerDbProvider<DkSourceDbContext>(Prefix + "D"),
            new SqlServerWrappedEfExtensions()
        )
        { }
    }
}