// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class NlContentResignExistingV1ContentCommand
    {
        private readonly NlContentResignCommand _Resigner;

        public NlContentResignExistingV1ContentCommand(NlContentResignCommand resigner)
        {
            _Resigner = resigner ?? throw new ArgumentNullException(nameof(resigner));
        }

        public async Task ExecuteAsync()
        {
            await _Resigner.ExecuteAsync(ContentTypes.ExposureKeySet, ContentTypes.ExposureKeySetV2, ZippedContentEntryNames.EksContent);
            await _Resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);
            await _Resigner.ExecuteAsync(ContentTypes.RiskCalculationParameters, ContentTypes.RiskCalculationParametersV2, ZippedContentEntryNames.Content);
            await _Resigner.ExecuteAsync(ContentTypes.ResourceBundle, ContentTypes.ResourceBundleV2, ZippedContentEntryNames.Content);
        }
    }
}