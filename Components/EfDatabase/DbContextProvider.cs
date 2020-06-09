// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public sealed class DbContextProvider<T> : IDbContextProvider<T> where T : DbContext
    {
        public T Current { get; }

        public DbContextProvider(T context)
        {
            Current = context;
        }

        public void Dispose()
        {
            Current?.Dispose();
        }
    }
}