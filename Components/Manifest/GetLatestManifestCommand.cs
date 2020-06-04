// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using EFCore.BulkExtensions;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class GetLatestManifestCommand
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IDbContextProvider<ExposureContentDbContext> _DbContext;
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IAgConfig _AgConfig;
        private readonly IPublishingIdCreator _PublishingIdCreator;

        public GetLatestManifestCommand(IUtcDateTimeProvider dateTimeProvider, IDbContextProvider<ExposureContentDbContext> dbContext, ManifestBuilder manifestBuilder, IAgConfig agConfig, IPublishingIdCreator publishingIdCreator)
        {
            _DateTimeProvider = dateTimeProvider;
            _DbContext = dbContext;
            _ManifestBuilder = manifestBuilder;
            _AgConfig = agConfig;
            _PublishingIdCreator = publishingIdCreator;
        }

        public ManifestEntity Execute()
        {
            var now = _DateTimeProvider.Now();
            var releaseCutoff = now - TimeSpan.FromHours(_AgConfig.ManifestLifeTimeHours);

            var e = _DbContext.Current.ManifestContent
                .Where(x => x.Release > releaseCutoff)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .SingleOrDefault();

            if (e != null)
                return e;

            _DbContext.Current.BulkDelete(_DbContext.Current.ManifestContent.ToList()); //TODO execute sql.
            var content = JsonConvert.SerializeObject(_ManifestBuilder.Execute());
            var bytes = Encoding.UTF8.GetBytes(content);

            e = new ManifestEntity
            { 
                Release = now,
                ContentTypeName = ContentHeaderValues.Json,
                Content = bytes,
                Region = DefaultValues.Region,
            };
            e.PublishingId = _PublishingIdCreator.Create(e);
            _DbContext.Current.ManifestContent.Add(e);
            _DbContext.SaveAndCommit();

            return e;
        }
    }
}