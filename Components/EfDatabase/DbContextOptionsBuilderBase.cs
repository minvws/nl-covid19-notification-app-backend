// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public abstract class DbContextOptionsBuilderBase : IDbContextOptionsBuilder
    {
        private readonly IEfDbConfig _EfDbConfig;
        private string _DatabaseName;

        protected DbContextOptionsBuilderBase(IEfDbConfig efDbConfig)
        {
            _EfDbConfig = efDbConfig;
        }

        protected string GetConnectionString()
            => _DatabaseName == null ? _EfDbConfig.ConnectionString : string.Format(_EfDbConfig.ConnectionString, _DatabaseName);

        public IDbContextOptionsBuilder AddDatabaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();

            if (_DatabaseName != null)
                throw new InvalidOperationException();

            _DatabaseName = name.Trim();

            return this;
        }

        public abstract DbContextOptions Build();
    }
}
