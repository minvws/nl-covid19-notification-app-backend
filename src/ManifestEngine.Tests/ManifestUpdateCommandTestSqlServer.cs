// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Tests
{
    [Trait("db", "ss")]
    [Collection(nameof(ManifestUpdateCommandTestSqlServer))]
    [ExclusivelyUses(nameof(ManifestUpdateCommandTestSqlServer))]
    public class ManifestUpdateCommandTestSqlServer : ManifestUpdateCommandTest
    {
        private const string Prefix = nameof(ManifestUpdateCommandTest) + "_";
        public ManifestUpdateCommandTestSqlServer()
            : base(new SqlServerDbProvider<ContentDbContext>(Prefix + "C"))
        { }
    }
}