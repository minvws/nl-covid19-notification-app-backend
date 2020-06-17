// Copyright ©  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public DynamicManifestReader(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId)
        {
            _ManifestBuilder = manifestBuilder;
            _DateTimeProvider = dateTimeProvider;
            _PublishingId = publishingId;
        }

        public async Task<ManifestEntity?> Execute(string _)
        {
            var now = _DateTimeProvider.Now();
            var e = new ManifestEntity
            {
                Release = now,
            };
            var content = _ManifestBuilder.Execute();
            //TODO HAX!
            var _Signer = new HardCodedSigner();
            var formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(_Signer), new StandardPublishingIdFormatter(_Signer));
            //End hax
            await formatter.Fill(e, content);
            return e;
        }
    }
}