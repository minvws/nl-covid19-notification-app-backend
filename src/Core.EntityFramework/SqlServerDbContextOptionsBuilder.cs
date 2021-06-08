// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public class SqlServerDbContextOptionsBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public SqlServerDbContextOptionsBuilder(IEfDbConfig efDbConfig, ILoggerFactory loggerFactory)
        {
            if (efDbConfig == null)
            {
                throw new ArgumentNullException(nameof(efDbConfig));
            }

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _connectionStringBuilder = new SqlConnectionStringBuilder(efDbConfig.ConnectionString)
            {
                MultipleActiveResultSets = true
            };
        }

        public DbContextOptions Build()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseLoggerFactory(_loggerFactory);
            builder.UseSqlServer(_connectionStringBuilder.ConnectionString);
            return builder.Options;
        }
    }
}
