// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace ManifestEngine.Tests
{
    [Trait("db", "ss")]
    [Collection(nameof(ManifestEngineTestsSqlite))]
    [ExclusivelyUses(nameof(ManifestEngineTestsSqlite))]
    public class ManifestEngineTestsSqlite : ManifestEngineTests
    {
        public ManifestEngineTestsSqlite()
            : base(new SqliteInMemoryDbProvider<ContentDbContext>())
        { }
    }
}
