// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Trait("db", "ss")]
    public class IksEngineTestsSqlserver : IksEngineTest
    {
        private const string Prefix = nameof(IksEngineTest) + "_";

        public IksEngineTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix+"W"),
            new SqlServerDbProvider<IksInDbContext>(Prefix + "II"),
            new SqlServerDbProvider<DkSourceDbContext>(Prefix + "D"),
            new SqlServerDbProvider<IksPublishingJobDbContext>(Prefix + "P"),
            new SqlServerDbProvider<IksOutDbContext>(Prefix + "IO"),
            new SqliteWrappedEfExtensions()
        )
        { }
    }
}