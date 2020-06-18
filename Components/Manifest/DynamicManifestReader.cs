// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class DynamicManifestReader : IReader<ManifestEntity>
    {
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IPublishingId _PublishingId;
        private readonly ISigner _Signer;

        public DynamicManifestReader(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId, ISigner signer)
        {
            _ManifestBuilder = manifestBuilder;
            _DateTimeProvider = dateTimeProvider;
            _PublishingId = publishingId;
            _Signer = signer;
        }

        public async Task<ManifestEntity?> Execute(string _)
        {
            var e = new ManifestEntity
            {
                Release = _DateTimeProvider.Now(),
            };
            var content = _ManifestBuilder.Execute();
            var formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(_Signer), new StandardPublishingIdFormatter(_Signer));
            await formatter.Fill(e, content);
            return e;
        }
    }
}