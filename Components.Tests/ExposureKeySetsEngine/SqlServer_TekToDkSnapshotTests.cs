// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    [Collection(nameof(SqlServer_TekToDkSnapshotTests))]
    [ExclusivelyUses(nameof(SqlServer_TekToDkSnapshotTests))]
    public class SqlServer_TekToDkSnapshotTests : TekToDkSnapshotTests
    {
        private const string Prefix = nameof(TekToDkSnapshotTests) + "_";
        public SqlServer_TekToDkSnapshotTests() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix+"W"),
            new SqlServerDbProvider<DkSourceDbContext>(Prefix + "D"),
            new SqlServerWrappedEfExtensions()
        )
        { }
    }
}