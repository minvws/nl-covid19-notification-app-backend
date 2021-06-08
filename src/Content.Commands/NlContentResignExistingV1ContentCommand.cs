// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class NlContentResignExistingV1ContentCommand
    {
        private readonly NlContentResignCommand _resigner;

        public NlContentResignExistingV1ContentCommand(NlContentResignCommand resigner)
        {
            _resigner = resigner ?? throw new ArgumentNullException(nameof(resigner));
        }

        public async Task ExecuteAsync()
        {
            await _resigner.ExecuteAsync(ContentTypes.ExposureKeySet, ContentTypes.ExposureKeySetV2, ZippedContentEntryNames.EksContent);
            await _resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);
            await _resigner.ExecuteAsync(ContentTypes.RiskCalculationParameters, ContentTypes.RiskCalculationParametersV2, ZippedContentEntryNames.Content);
            await _resigner.ExecuteAsync(ContentTypes.ResourceBundle, ContentTypes.ResourceBundleV2, ZippedContentEntryNames.Content);
        }
    }
}