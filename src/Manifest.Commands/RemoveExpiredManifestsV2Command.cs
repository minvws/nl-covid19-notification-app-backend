// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class RemoveExpiredManifestsV2Command
    {
        private readonly RemoveExpiredManifestsReceiver _receiver;
        private readonly ExpiredManifestV2LoggingExtensions _logger;

        public RemoveExpiredManifestsV2Command(Func<ContentDbContext> dbContextProvider, ExpiredManifestV2LoggingExtensions logger, IManifestConfig manifestConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> ExecuteAsync()
        {
            return await _receiver.RemoveManifests(ContentTypes.ManifestV2, _logger);
        }
    }
}
