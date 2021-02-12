// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class DefaultTekValidatorConfig : ITekValidatorConfig
    {
        public int RollingStartNumberMin => new DateTime(2020, 7, 1, 0, 0, 0, DateTimeKind.Utc).ToRollingStartNumber();
        public int MaxAgeDays => 14;
        public int PublishingDelayInMinutes => 120; //NB changing this breaks a unit test cos tests dont read settings.
        public int AuthorisationWindowMinutes => 120;
    }
}