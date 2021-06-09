// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsV3Command
    {
        private readonly RemoveExpiredManifestsReceiver _receiver;
        private readonly ExpiredManifestV3LoggingExtensions _logger;

        public RemoveExpiredManifestsV3Command(ExpiredManifestV3LoggingExtensions logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> ExecuteAsync()
        {
            return await _receiver.RemoveManifests(ContentTypes.ManifestV3, _logger);
        }
    }
}
