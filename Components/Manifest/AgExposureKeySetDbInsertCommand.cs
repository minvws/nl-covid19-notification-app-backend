// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class AgExposureKeySetDbInsertCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _DbConfig;
        private readonly IPublishingIdCreator _PublishingIdCreator;

        public AgExposureKeySetDbInsertCommand(IDbContextProvider<ExposureContentDbContext> config, IPublishingIdCreator publishingIdCreator)
        {
            _DbConfig = config;
            _PublishingIdCreator = publishingIdCreator;
        }

        public async Task<ExposureKeySetContentEntity> Execute(ExposureKeySetContentEntity e)
        {
            e.PublishingId = _PublishingIdCreator.Create(e);
            await _DbConfig.Current.AddAsync(e);
            return e;
        }
    }
}