// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class SqlServerDbContextOptionsBuilder : IDbContextOptionsBuilder
    {
        private readonly IEfDbConfig _EfDbConfig;
        private string _DatabaseName;

        public SqlServerDbContextOptionsBuilder(IEfDbConfig efDbConfig)
        {
            _EfDbConfig = efDbConfig;
        }

        protected string GetConnectionString()
            => string.IsNullOrEmpty(_DatabaseName) ? _EfDbConfig.ConnectionString : string.Format(_EfDbConfig.ConnectionString, _DatabaseName);

        public IDbContextOptionsBuilder AddDatabaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();

            if (_DatabaseName != null)
                throw new InvalidOperationException();

            _DatabaseName = name.Trim();

            return this;
        }

        public DbContextOptions Build()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlServer(GetConnectionString());
            return builder.Options;
        }
    }
}