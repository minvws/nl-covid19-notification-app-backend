// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidator : ITemporaryExposureKeyValidator
    {
        private readonly ITekValidatorConfig _Config;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public TemporaryExposureKeyValidator(ITekValidatorConfig config, IUtcDateTimeProvider dateTimeProvider)
        {
            _Config = config;
            _DateTimeProvider = dateTimeProvider;
        }

        public string[] Valid(PostTeksItemArgs value)
        {
            if (value == null)
                return new [] {"Value is null."};

            var result = new List<string>();

            var earliestAcceptedDateFromDevices = _DateTimeProvider.Snapshot.Date - TimeSpan.FromDays(_Config.MaxAgeDays);
            var rollingStartMin = Math.Max(_Config.RollingStartNumberMin, earliestAcceptedDateFromDevices.ToRollingStartNumber());
            var rollingStartToday = _DateTimeProvider.Snapshot.Date.ToRollingStartNumber();

            if (!(rollingStartMin <= value.RollingStartNumber && value.RollingStartNumber <= rollingStartToday))
                result.Add($"RollingStartNumber out of range - {value.RollingStartNumber}.");

            if (!(_Config.RollingPeriodMin <= value.RollingPeriod && value.RollingPeriod <= _Config.RollingPeriodMax))
                result.Add($"RollingPeriod out of range - {value.RollingPeriod}.");

            if (string.IsNullOrEmpty(value.KeyData))
                result.Add("KeyData is empty.");

            var buffer = new Span<byte>(new byte[_Config.KeyDataLength]);
            if (!Convert.TryFromBase64String(value.KeyData, buffer, out var count))
                result.Add("KeyData is not valid base64.");

            if (_Config.KeyDataLength != count)
                result.Add($"KeyData length incorrect - {count}.");

            return result.ToArray();
        }
    }
}