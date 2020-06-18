// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class SafeBinaryContentDbReader<T> : IReader<T> where T : ContentEntity
    {
        private readonly ExposureContentDbContext _DbContextProvider;

        public SafeBinaryContentDbReader(ExposureContentDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public async Task<T?> Execute(string id)
        {

            return _DbContextProvider.Set<T>()
                .SingleOrDefault(x => x.PublishingId == id);

            //if (e == null)
            //    return null;

            //return new BinaryContentResponse
            //{
            //    LastModified = e.Release,
            //    PublishingId = e.PublishingId,
            //    ContentTypeName = e.ContentTypeName,
            //    Content = e.Content
            //};
        }
    }
}