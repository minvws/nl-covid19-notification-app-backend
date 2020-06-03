// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class ExposureContentDbContext : DbContext
    {
        public ExposureContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ManifestEntity> ManifestContent { get; set; }
        public DbSet<ExposureKeySetContentEntity> ExposureKeySetContent { get; set; }
        public DbSet<RiskCalculationContentEntity> RiskCalculationulationContent { get; set; }
        public DbSet<RivmAdviceContentEntity> RivmAdviceContent { get; set; }
    }
}
