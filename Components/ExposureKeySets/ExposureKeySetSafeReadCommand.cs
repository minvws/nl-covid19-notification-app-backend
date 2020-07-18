// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    public class ExposureKeySetSafeReadCommand
    {
        private readonly ContentDbContext _DbConfig;

        public ExposureKeySetSafeReadCommand(ContentDbContext dbConfig)
        {
            _DbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
        }

        /// <summary>
        /// Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExposureKeySetContentEntity Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));

            return _DbConfig.Set<ExposureKeySetContentEntity>()
                .Where(x => x.PublishingId == id)
                .Take(1)
                .ToArray()
                .SingleOrDefault();
        }
    }
}
