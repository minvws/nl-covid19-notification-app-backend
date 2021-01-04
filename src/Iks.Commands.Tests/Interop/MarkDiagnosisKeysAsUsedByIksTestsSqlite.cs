// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Trait("db", "mem")]
    public class MarkDiagnosisKeysAsUsedByIksTestsSqlite : MarkDiagnosisKeysAsUsedByIksTests
    {
        public MarkDiagnosisKeysAsUsedByIksTestsSqlite() : base(
            new SqliteInMemoryDbProvider<DkSourceDbContext>(),
            new SqliteInMemoryDbProvider<IksPublishingJobDbContext>()
        )
        { }
    }
}