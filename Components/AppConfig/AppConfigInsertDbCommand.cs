// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigInsertDbCommand
    {
        private readonly ExposureContentDbContext _DbConfig;
        private readonly IPublishingId _PublishingId;

        public AppConfigInsertDbCommand(ExposureContentDbContext dbConfig, IPublishingId publishingId)
        {
            _DbConfig = dbConfig;
            _PublishingId = publishingId;
        }

        public async Task Execute(AppConfigArgs args)
        {
            var e = args.ToEntity();
            e.PublishingId = _PublishingId.Create(e);
            await _DbConfig.AddAsync(e);
        }
    }
}
