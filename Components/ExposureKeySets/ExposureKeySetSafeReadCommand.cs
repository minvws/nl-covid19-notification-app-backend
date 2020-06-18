// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    public class ExposureKeySetSafeReadCommand
    {
        private readonly ExposureContentDbContext _DbConfig;

        public ExposureKeySetSafeReadCommand(ExposureContentDbContext dbConfig)
        {
            _DbConfig = dbConfig;
        }

        /// <summary>
        /// Returns null if not found.
        /// </summary>
        /// <param name="exposureKeySetId"></param>
        /// <returns></returns>
        public ExposureKeySetContentEntity Execute(string exposureKeySetId)
        {
            return _DbConfig.Set<ExposureKeySetContentEntity>()
                .Where(x => x.PublishingId == exposureKeySetId)
                .Take(1)
                .ToArray()
                .SingleOrDefault();
        }
    }
}
