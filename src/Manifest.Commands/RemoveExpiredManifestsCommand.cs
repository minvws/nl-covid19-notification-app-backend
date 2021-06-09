// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsCommand
    {
        private readonly RemoveExpiredManifestsReceiver _receiver;
        private readonly ExpiredManifestLoggingExtensions _logger;

        public RemoveExpiredManifestsCommand(RemoveExpiredManifestsReceiver receiver, ExpiredManifestLoggingExtensions logger)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> ExecuteAsync()
        {
            return await _receiver.RemoveManifests(ContentTypes.Manifest, _logger);
        }
    }
}
