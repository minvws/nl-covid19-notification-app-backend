// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class SafeBinaryContentDbReader<T> : IReader<T> where T : ContentEntity
    {
        private readonly ContentDbContext _DbContextProvider;

        public SafeBinaryContentDbReader(ContentDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
        }

        public async Task<T?> Execute(string id)
        {
            return _DbContextProvider.Set<T>()
                .SingleOrDefault(x => x.PublishingId == id);
        }
    }
}