// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public static class DbContextExtras
    {

        /// <summary>
        /// Most likely called by batch jobs running in web applications where the context starts a TX at the beginning of the request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T EnsureNoChangesOrTransaction<T>(this T context) where T: DbContext
        { 
            if (context.ChangeTracker.HasChanges())
                throw new InvalidOperationException("Db context has unsaved changes.");

            if (context.Database.CurrentTransaction != null)
                context.Database.RollbackTransaction();

            return context;
        }

        public static IDbContextTransaction BeginTransaction(this DbContext context)
        {
            if (context.Database.CurrentTransaction != null)
                throw new InvalidOperationException("Database has existing transaction.");

            return context.Database.BeginTransaction();
        }

        public static void SaveAndCommit(this DbContext context)
        {
            if (context.Database.CurrentTransaction == null)
                throw new InvalidOperationException("No current transaction.");

            context.SaveChanges();
            context.Database.CurrentTransaction.Commit();
        }
    }
}