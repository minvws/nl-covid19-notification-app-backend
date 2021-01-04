// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework
{
    public class IksPublishingJobDbContext : DbContext
    {
        public IksPublishingJobDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<IksCreateJobInputEntity> Input { get; set; }

        public DbSet<IksCreateJobOutputEntity> Output { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            //TODO indexes cos filters...
        }
    }
}