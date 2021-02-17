// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    [Trait("db", "mem")]
    public class RemoveExpiredManifestsTestSqlite : RemoveExpiredManifestsTest
    {
        public RemoveExpiredManifestsTestSqlite() : base(new SqliteInMemoryDbProvider<ContentDbContext>())
        { }
    }
}