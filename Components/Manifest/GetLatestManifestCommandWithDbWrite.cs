// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    [Obsolete("Writes to DB too.")]
    public class GetLatestManifestCommandWithDbWrite
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ExposureContentDbContext _DbContext;
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IGaenContentConfig _GaenContentConfig;
        private readonly IPublishingId _PublishingId;
        private readonly IJsonSerializer _JsonSerializer;

        public GetLatestManifestCommandWithDbWrite(IUtcDateTimeProvider dateTimeProvider, ExposureContentDbContext dbContext, ManifestBuilder manifestBuilder, IGaenContentConfig gaenContentConfig, IPublishingId publishingId, IJsonSerializer jsonSerializer)
        {
            _DateTimeProvider = dateTimeProvider;
            _DbContext = dbContext;
            _ManifestBuilder = manifestBuilder;
            _GaenContentConfig = gaenContentConfig;
            _PublishingId = publishingId;
            _JsonSerializer = jsonSerializer;
        }

        public ManifestEntity Execute()
        {
            _DbContext.BeginTransaction(); //TODO should be using WebDbContentProvider

            var now = _DateTimeProvider.Now();
            var releaseCutoff = now - TimeSpan.FromHours(_GaenContentConfig.ManifestLifetimeHours);

            var e = _DbContext.ManifestContent
                .Where(x => x.Release > releaseCutoff)
                .OrderByDescending(x => x.Release)
                .Take(1)
                .SingleOrDefault();

            if (e != null)
                return e;

            _DbContext.BulkDelete(_DbContext.Set<ManifestEntity>().ToList()); //TODO execute sql.
            var content =  _JsonSerializer.Serialize(_ManifestBuilder.Execute());
            var bytes = Encoding.UTF8.GetBytes(content);

            e = new ManifestEntity
            { 
                Release = now,
                ContentTypeName = ContentHeaderValues.Json,
                Content = bytes,
            };
            e.PublishingId = _PublishingId.Create(e.Content);
            _DbContext.ManifestContent.Add(e);
            _DbContext.SaveAndCommit();

            return e;
        }
    }
}