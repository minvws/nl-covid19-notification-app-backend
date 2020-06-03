// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class SqlServerDbContextOptionsBuilder : DbContextOptionsBuilderBase
    {
        public SqlServerDbContextOptionsBuilder(IEfDbConfig efDbConfig) : base(efDbConfig)
        {
        }

        public override DbContextOptions Build()
        {
            var inner = new DbContextOptionsBuilder();
            inner.UseSqlServer(GetConnectionString());
            //TODO Anything else? var builder = new SqlServerDbContextOptionsBuilder(inner);
            return inner.Options;
        }
    }
}