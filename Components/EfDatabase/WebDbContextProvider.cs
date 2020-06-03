// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    /// <summary>
    /// One transaction per request.
    /// Add to services with Scoped lifetime
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebDbContextProvider<T> : IDbContextProvider<T> where T : DbContext
    {
        public WebDbContextProvider(Func<T> build)
        {
            Current = build();
            Current.Database.AutoTransactionsEnabled = false;
            Current.Database.BeginTransaction();
        }

        public T Current { get; }

        public void Commit()
        {
            Current.SaveChanges();
            Current.Database.CurrentTransaction.Commit();
        }

        public void Dispose()
        {
            Current?.Database?.CurrentTransaction?.Dispose();
        }
    }
}