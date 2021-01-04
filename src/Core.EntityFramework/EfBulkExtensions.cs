// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public static class EfBulkExtensions
    {
        public static async Task BulkUpdateAsync2<T>(this DbContext db, IList<T> page, SubsetBulkArgs args) where T:class
        {
            await using (db.BeginTransaction())
            {
                await db.BulkUpdateAsync(page, args.ToBulkConfig());
                db.SaveAndCommit();
            }
        }

        public static async Task BulkInsertAsync2<T>(this DbContext db, IList<T> page, SubsetBulkArgs args) where T : class
        {
            await using (db.BeginTransaction())
            {
                await db.BulkInsertAsync(page, args.ToBulkConfig());
                db.SaveAndCommit();
            }
        }
    }
}