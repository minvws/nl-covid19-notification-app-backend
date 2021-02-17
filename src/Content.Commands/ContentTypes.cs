// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public static class ContentTypes
    {
        public const string ResourceBundle = nameof(ResourceBundle);

        public const string AppConfig = nameof(AppConfig);
        public const string RiskCalculationParameters = nameof(RiskCalculationParameters);
        public const string ExposureKeySet = nameof(ExposureKeySet);
        public const string Manifest = nameof(Manifest);

        public const string AppConfigV2 = nameof(AppConfigV2);
        public const string RiskCalculationParametersV2 = nameof(RiskCalculationParametersV2);
        public const string ExposureKeySetV2 = nameof(ExposureKeySetV2);
        public const string ManifestV2 = nameof(ManifestV2);
        public const string ResourceBundleV2 = nameof(ResourceBundleV2);

        public const string ManifestV3 = nameof(ManifestV3);
        public const string ResourceBundleV3 = nameof(ResourceBundleV3); //refers to the ResourceBundleV2-files in publishcontent-repo.

        //New for GAENv2
        public const string RiskCalculationParametersV3 = nameof(RiskCalculationParametersV3);
        public const string ManifestV4 = nameof(ManifestV4);

        public static bool IsValid(string value) =>
            value == AppConfig
            || value == RiskCalculationParameters
            || value == ExposureKeySet
            || value == Manifest

            || value == AppConfigV2
            || value == RiskCalculationParametersV2
            || value == ExposureKeySetV2
            || value == ManifestV2

            || value == ResourceBundle
            || value == ResourceBundleV2
            || value == ManifestV3
            || value == ResourceBundleV3

            || value == RiskCalculationParametersV3
            || value == ManifestV4
        ;
    }
}