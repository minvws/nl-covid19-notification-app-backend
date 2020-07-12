// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    //Reads and formats...
    public class DynamicManifestReader
    {
        private readonly ManifestBuilder _ManifestBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IContentSigner _ContentSigner;
        private readonly ILogger _Logger; //Actually not used.

        public DynamicManifestReader(ManifestBuilder manifestBuilder, IUtcDateTimeProvider dateTimeProvider, IContentSigner contentSigner, ILogger logger)
        {
            _ManifestBuilder = manifestBuilder;
            _DateTimeProvider = dateTimeProvider;
            _ContentSigner = contentSigner;
            _Logger = logger;
        }

        public async Task<ManifestEntity?> Execute()
        {
            var e = new ManifestEntity
            {
                Release = _DateTimeProvider.Now(),
            };
            _Logger.Debug("Build new manifest.");
            var content = _ManifestBuilder.Execute();
            //TODO should be injected...
            _Logger.Debug("Format and sign new manifest.");
            var formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(_ContentSigner), new StandardPublishingIdFormatter());
            await formatter.Fill(e, content); //TODO add release date as a parameter
            return e;
        }
    }
}