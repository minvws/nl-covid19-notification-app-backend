// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public static class ContentTypes
    {
        public const string AppConfig = nameof(AppConfig);
        public const string RiskCalculationParameters = nameof(RiskCalculationParameters); //TODO API version?

        public static bool IsValid(string value) => value == AppConfig || value == RiskCalculationParameters;
    }
}