// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class IccBackendContentDbContextFactory : IDesignTimeDbContextFactory<IccBackendContentDbContext>
    {
        public IccBackendContentDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IccBackendContentDbContext>();
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Icc;Integrated Security=true",o =>
            {
                o.MigrationsHistoryTable("__MigrationHistory", "dbo");
            });

            return new IccBackendContentDbContext(optionsBuilder.Options);
        }
    }
}