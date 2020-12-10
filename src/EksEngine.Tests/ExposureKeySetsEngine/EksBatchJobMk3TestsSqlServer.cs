// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "ss")]
    [Collection(nameof(EksBatchJobMk3TestsSqlServer))]
    public class EksBatchJobMk3TestsSqlServer : EksBatchJobMk3Tests
    {
        private const string DbNamePrefix = nameof(EksBatchJobMk3Tests) + "_";

        public EksBatchJobMk3TestsSqlServer() : base(
            new SqlServerDbProvider<WorkflowDbContext>(DbNamePrefix + "W"),
            new SqlServerDbProvider<DkSourceDbContext>(DbNamePrefix + "D"),
            new SqlServerDbProvider<EksPublishingJobDbContext>(DbNamePrefix + "P"),
            new SqlServerDbProvider<ContentDbContext>(DbNamePrefix + "C"),
            new SqlServerWrappedEfExtensions())
        { }
    }
}