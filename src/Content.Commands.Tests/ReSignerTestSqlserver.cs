// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    [Trait("db", "ss")]
    public class ReSignerTestSqlserver : ReSignerTest
    {
        //private const string Prefix = nameof(ReSignerTest) + "_";
        //public ReSignerTestSqlserver() : base(
        //    new SqlServerDbProvider<ContentDbContext>(Prefix + "C")
        //)
        //{ }
        public ReSignerTestSqlserver(DbContextOptions<ContentDbContext> contentDbContextOptions) : base(contentDbContextOptions)
        {
        }
    }
}
