// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace EksEngine.Tests
{
    [Collection(nameof(SqlServerEksEngineTests))]
    [ExclusivelyUses(nameof(SqlServerEksEngineTests))]
    public class SqlServerEksEngineTests : EksEngineTests
    {
        private const string Prefix = nameof(EksEngineTests)+"_";
        public SqlServerEksEngineTests()
            : base(
                new SqlServerDbProvider<WorkflowDbContext>(Prefix+"W"),
                new SqlServerDbProvider<DkSourceDbContext>(Prefix + "DK"),
                new SqlServerDbProvider<EksPublishingJobDbContext>(Prefix + "EP"),
                new SqlServerDbProvider<ContentDbContext>(Prefix + "C"),
                new SqlServerWrappedEfExtensions()
            )
        { }
    }
}