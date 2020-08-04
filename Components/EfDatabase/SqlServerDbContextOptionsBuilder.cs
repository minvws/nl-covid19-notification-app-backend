// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class SqlServerDbContextOptionsBuilder 
    {
        private readonly SqlConnectionStringBuilder _ConnectionStringBuilder;

        public SqlServerDbContextOptionsBuilder(IEfDbConfig efDbConfig)
        {
            if (efDbConfig == null) throw new ArgumentNullException(nameof(efDbConfig));

            _ConnectionStringBuilder = new SqlConnectionStringBuilder(efDbConfig.ConnectionString) 
            {
                    MultipleActiveResultSets = true
            };
        }

        public DbContextOptions Build()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer(_ConnectionStringBuilder.ConnectionString);
            return builder.Options;
        }
    }
}