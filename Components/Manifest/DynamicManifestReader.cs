// Copyright ©  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class DynamicManifestReader : IReader<ManifestEntity>
    {
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IPublishingId _PublishingId;

        public DynamicManifestReader(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId)
        {
            _ManifestBuilder = manifestBuilder;
            _DateTimeProvider = dateTimeProvider;
            _PublishingId = publishingId;
        }

        public ManifestEntity? Execute(string _)
        {
            var now = _DateTimeProvider.Now();
            var r = _ManifestBuilder.Execute();
            var content = JsonConvert.SerializeObject(r);
            var bytes = Encoding.UTF8.GetBytes(content);

            var result = new ManifestEntity
            {
                Release = now,
                Content = bytes,
                ContentTypeName = ContentHeaderValues.Json,
            };
            result.PublishingId = _PublishingId.Create(result);
            return result;
        }
    }
}