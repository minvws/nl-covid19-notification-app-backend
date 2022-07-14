// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework
{
    public class IksOutDbContext : DbContext
    {
        public IksOutDbContext(DbContextOptions<IksOutDbContext> options)
            : base(options)
        {
        }

        public DbSet<IksOutEntity> Iks { get; set; }
    }
}
