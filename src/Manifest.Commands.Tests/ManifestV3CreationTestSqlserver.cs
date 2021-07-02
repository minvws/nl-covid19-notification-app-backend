// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    [Trait("db", "ss")]
    public class ManifestV3CreationTestSqlserver : ManifestV3CreationTest
    {
        //private const string Prefix = nameof(ManifestV3CreationTest) + "_";
        //public ManifestV3CreationTestSqlserver() : base(
        //    new SqlServerDbProvider<ContentDbContext>(Prefix + "C")
        //)
        //{ }
        public ManifestV3CreationTestSqlserver(DbContextOptions<ContentDbContext> contentDbContextOptions) : base(contentDbContextOptions)
        {
        }
    }
}
