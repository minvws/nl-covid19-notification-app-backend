// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Efgs.Tests.SqlServer
{
    [Collection(nameof(SqlServer_EksFromDksTest))]
    [ExclusivelyUses(nameof(SqlServer_EksFromDksTest))]
    public class SqlServer_EksFromDksTest : EksFromDksTest
    {
        private const string TestsName = nameof(EksFromDksTest);

        public SqlServer_EksFromDksTest() : base(
            new SqlServerDbProvider<WorkflowDbContext>(TestsName+"W"),
            new SqlServerDbProvider<DkSourceDbContext>(TestsName + "DK"),
            new SqlServerDbProvider<EksPublishingJobDbContext>(TestsName + "P"),
            new SqlServerDbProvider<ContentDbContext>(TestsName + "C"),
            new SqlServerWrappedEfExtensions()
        )
        { }
    }
}