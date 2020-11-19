// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace ManifestEngine.Tests
{
    [Collection(nameof(Sqlite_ManifestEngineTests))]
    [ExclusivelyUses(nameof(Sqlite_ManifestEngineTests))]
    public class Sqlite_ManifestEngineTests : ManifestEngineTests
    {
        public Sqlite_ManifestEngineTests()
            : base(new SqliteInMemoryDbProvider<ContentDbContext>())
        { }
    }
}
