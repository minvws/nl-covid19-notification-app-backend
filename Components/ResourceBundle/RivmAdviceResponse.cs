// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice
{
    public class RivmAdviceResponse
    {
        public string Id { get; set; }
        public int TemporaryExposureKeyRetentionDays { get; set; }
        public int ObservedTemporaryExposureKeyRetentionDays { get; set; }
        public int IsolationPeriodDays { get; set; }
        public LocalizableText[] Text { get; set; }
    }
}