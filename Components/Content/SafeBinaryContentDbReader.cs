// Copyright ©  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Data;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public interface IReader<out T> where T : ContentEntity
    {
        T? Execute(string id);
    }

    public class SafeBinaryContentDbReader<T> : IReader<T> where T : ContentEntity
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _DbContextProvider;

        public SafeBinaryContentDbReader(IDbContextProvider<ExposureContentDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public T? Execute(string id)
        {

            return _DbContextProvider.Current.Set<T>()
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