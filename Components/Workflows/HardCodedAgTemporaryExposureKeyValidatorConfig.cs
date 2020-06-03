// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class HardCodedAgTemporaryExposureKeyValidatorConfig : ITemporaryExposureKeyValidatorConfig
    {
        public int RollingPeriodMin => 1;
        public int RollingPeriodMax => int.MaxValue;
        public int DailyKeyByteCount => 32; //TODO change to real value and use this setting in generator
    }
}