// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class SqlServerDbContextOptionsBuilder : IDbContextOptionsBuilder
    {
        private readonly SqlConnectionStringBuilder _ConnectionStringBuilder;

        public SqlServerDbContextOptionsBuilder(IEfDbConfig efDbConfig)
        {
            _ConnectionStringBuilder = new SqlConnectionStringBuilder(efDbConfig.ConnectionString) 
            {
                    MultipleActiveResultSets = true
            };
        }

        //TODO use the other ctor...
        public SqlServerDbContextOptionsBuilder(string connectionString)
        {
            _ConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString) 
            {
                MultipleActiveResultSets = true
            };
        }

        public IDbContextOptionsBuilder AddDatabaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();

            _ConnectionStringBuilder.InitialCatalog = name.Trim();
            return this;
        }

        public DbContextOptions Build()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer(_ConnectionStringBuilder.ConnectionString);
            return builder.Options;
        }
    }
}