// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class DefaultGaenTekValidatorConfig : ITemporaryExposureKeyValidatorConfig
    {
        public int RollingPeriodMin => 1;
        public int RollingPeriodMax => Int32.MaxValue;
        public int DailyKeyByteCount => 32;
    }
}