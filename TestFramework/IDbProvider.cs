// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework
{
    /// <summary>
    /// Enables fast testing using in-memory Sqlite for quick feedback and slower testing with SqlServer to ensure compatibility with the target architecture.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public interface IDbProvider<TDbContext> : IDisposable where TDbContext : DbContext
    {
        Func<TDbContext> CreateNew { get; }
        Func<TDbContext> CreateNewWithTx { get; }
    }
}