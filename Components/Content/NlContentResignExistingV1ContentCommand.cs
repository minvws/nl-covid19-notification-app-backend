// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class NlContentResignExistingV1ContentCommand
    {
        private readonly NlContentResignCommand _Resigner;
        private readonly IThumbprintConfig _V2ThumbprintConfig;
        private readonly ILogger<NlContentResignExistingV1ContentCommand> _Logger;

        public NlContentResignExistingV1ContentCommand(NlContentResignCommand resigner, IThumbprintConfig v2ThumbprintConfig, ILogger<NlContentResignExistingV1ContentCommand> logger)
        {
            _Resigner = resigner ?? throw new ArgumentNullException(nameof(resigner));
            _V2ThumbprintConfig = v2ThumbprintConfig ?? throw new ArgumentNullException(nameof(v2ThumbprintConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute()
        {
            if (!_V2ThumbprintConfig.Valid)
            {
                _Logger.LogWarning("Certificate for re-signing not specified in settings. Re-signing will not run.");
                return;
            }

            await _Resigner.Execute(ContentTypes.Manifest, ContentTypes.ManifestV2, ZippedContentEntryNames.Content);
            await _Resigner.Execute(ContentTypes.ExposureKeySet, ContentTypes.ExposureKeySetV2, ZippedContentEntryNames.EksContent);
            await _Resigner.Execute(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);
            await _Resigner.Execute(ContentTypes.RiskCalculationParameters, ContentTypes.RiskCalculationParametersV2, ZippedContentEntryNames.Content);
        }
    }
}