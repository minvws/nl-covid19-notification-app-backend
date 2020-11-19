// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework
{
    public class SqliteWrappedEfExtensions : IWrappedEfExtensions
    {
        public void TruncateTable(DbContext context, string tableName)
        {
            //NB ExecuteSqlInterpolated throws with 'the table does not exist'.
            context.Database.ExecuteSqlRaw($"DELETE FROM {tableName}");
        }
    }
}