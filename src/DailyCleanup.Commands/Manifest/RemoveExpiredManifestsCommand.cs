// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Manifest
{
    public class RemoveExpiredManifestsCommand : BaseCommand
    {
        private readonly RemoveExpiredManifestsReceiver _receiver;

        private readonly List<ContentTypes> _manifestTypes = new List<ContentTypes>
        {
            ContentTypes.ManifestV2,
            ContentTypes.ManifestV3,
            ContentTypes.ManifestV4,
            ContentTypes.ManifestV5
        };

        public RemoveExpiredManifestsCommand(RemoveExpiredManifestsReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public override async Task<ICommandResult> ExecuteAsync()
        {
            foreach (var item in _manifestTypes)
            {
                _ = await _receiver.RemoveManifestsAsync(manifestType: item);
            }

            return null;
        }
    }
}
