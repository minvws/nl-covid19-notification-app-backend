// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ContentLoading;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public static class DbContextQueries
    {
        public static async Task<T?> SafeGetContent<T>(this DbContext dbContextProvider, string id) where T : ContentEntity
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<T>().SingleOrDefaultAsync(x => x.PublishingId == id);
        }

        public static async Task<GenericContentEntity> SafeGetGenericContent(this DbContext dbContextProvider, string genericType, string id)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<GenericContentEntity>().SingleOrDefaultAsync(x => x.PublishingId == id && x.GenericType == genericType);
        }

        public static async Task<T> SafeGetLatestContent<T>(this DbContext dbContextProvider, DateTime now) where T:ContentEntity
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<T>()
                .Where(x => x.Release < now)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .SingleOrDefaultAsync();
        }

        public static async Task<string> SafeGetLatestGenericContentId(this DbContext dbContextProvider, string genericType, DateTime now)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            return await dbContextProvider.Set<GenericContentEntity>()
                .Where(x => x.Release < now && x.GenericType == genericType)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .Select(x => x.PublishingId)
                .SingleOrDefaultAsync() ?? string.Empty;
        }

        public static async Task<string[]> SafeGetActiveContentIdList<T>(this DbContext dbContextProvider, DateTime from, DateTime to) where T : ContentEntity
        {
            var result = (await dbContextProvider.Set<T>()
                .Where(x => x.Release >= from && x.Release <= to)
                .Select(x => x.PublishingId)
                .ToArrayAsync());

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return result;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

    }
}