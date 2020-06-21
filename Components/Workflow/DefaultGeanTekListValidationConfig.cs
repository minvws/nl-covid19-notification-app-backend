// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class DefaultGeanTekListValidationConfig : IGeanTekListValidationConfig
    {
        public int TemporaryExposureKeyCountMin => 1;
        public int TemporaryExposureKeyCountMax => 21;
        public int GracePeriod => 30;
    }
}