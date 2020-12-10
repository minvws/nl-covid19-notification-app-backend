// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    [Collection(nameof(MarkDiagnosisKeysAsUsedByIksTestsSqlServer))]
    [ExclusivelyUses(nameof(MarkDiagnosisKeysAsUsedByIksTestsSqlServer))]
    [Trait("db", "ss")]
    public class MarkDiagnosisKeysAsUsedByIksTestsSqlServer : MarkDiagnosisKeysAsUsedByIksTests
    {
        private const string Prefix = nameof(MarkDiagnosisKeysAsUsedByIksTests) + "_";
        public MarkDiagnosisKeysAsUsedByIksTestsSqlServer() : base(
            new SqlServerDbProvider<DkSourceDbContext>(Prefix + "D"),
            new SqlServerDbProvider<IksPublishingJobDbContext>(Prefix+"IPJ")
        )
        { }
    }
}