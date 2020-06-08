// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    public class ExposureKeySetSafeReadCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbConfig;

        public ExposureKeySetSafeReadCommand(IDbContextProvider<ExposureContentDbContext>dbConfig)
        {
            _DbConfig = dbConfig;
        }

        /// <summary>
        /// Returns null if not found.
        /// </summary>
        /// <param name="ExposureKeySetId"></param>
        /// <returns></returns>
        public ExposureKeySetContentEntity Execute(string ExposureKeySetId)
        {
            return _DbConfig.Current.Set<ExposureKeySetContentEntity>()
                .Where(x => x.PublishingId == ExposureKeySetId)
                .Take(1)
                .ToArray()
                .SingleOrDefault();
        }
    }
}
