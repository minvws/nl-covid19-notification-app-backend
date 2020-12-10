// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public static class DbContextQueries
    {

        /// <summary>
        /// e.g. GET Immutable content
        /// NB - content can be repeated with a different Publishing date.
        /// </summary>
        public static async Task<ContentEntity> SafeGetContentAsync(this DbContext dbContextProvider, string type, string id, DateTime now)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<ContentEntity>()
                .Where(x => x.PublishingId == id && x.Type == type && x.Release < now )
                .OrderByDescending(x => x.Release)
                .Take(1)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// e.g. GET Manifest
        /// </summary>
        public static async Task<ContentEntity?> SafeGetLatestContentAsync(this DbContext dbContextProvider, string type, DateTime now)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<ContentEntity>()
                .Where(x => x.Release < now && x.Type == type)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Build manifest - non-EKS
        /// </summary>
        public static async Task<string> SafeGetLatestContentIdAsync(this DbContext dbContextProvider, string type, DateTime now)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<ContentEntity>()
                .Where(x => x.Release < now && x.Type == type)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .Select(x => x.PublishingId)
                .SingleOrDefaultAsync() ?? string.Empty;
        }

        /// <summary>
        /// Build manifest - EKS
        /// </summary>
        public static async Task<string[]> SafeGetActiveContentIdListAsync(this DbContext dbContextProvider, string type, DateTime from, DateTime to)
        {
            var result = await dbContextProvider.Set<ContentEntity>()
                .Where(x => x.Release >= from && x.Release <= to && x.Type == type)
                .Select(x => x.PublishingId)
                .ToArrayAsync();

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return result;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

    }
}