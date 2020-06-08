// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    /// <summary>
    /// Use for one-transaction-per-request in Web Apps
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbContextProvider<T> : IDbContextProvider<T> where T : DbContext
    {
        private readonly Func<T> _Build;

        private T? _Current;

        public T Current => _Current ??= _Build();

        public DbContextProvider(Func<T> build)
        {
            _Build = build;
        }

        public void Dispose()
        {
            Current?.Dispose();
        }
    }
}