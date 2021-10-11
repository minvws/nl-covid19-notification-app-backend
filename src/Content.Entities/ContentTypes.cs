// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities
{
    public enum ContentTypes
    {
        ResourceBundle,
        AppConfig,
        RiskCalculationParameters,
        ExposureKeySet,
        Manifest,
        AppConfigV2,
        RiskCalculationParametersV2,
        ExposureKeySetV2,
        ManifestV2,
        ResourceBundleV2,
        ManifestV3,
        ResourceBundleV3, //refers to the ResourceBundleV2-files in publishcontent-repo.

        //New for GAENv2
        RiskCalculationParametersV3,
        ManifestV4,
        ManifestV5,
        ExposureKeySetV3,
    }
}
