// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands
{
    public class TemporaryExposureKeyValidator : ITemporaryExposureKeyValidator
    {
        private readonly ITekValidatorConfig _config;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        public TemporaryExposureKeyValidator(ITekValidatorConfig config, IUtcDateTimeProvider dateTimeProvider)
        {
            _config = config;
            _dateTimeProvider = dateTimeProvider;
        }

        public string[] Valid(PostTeksItemArgs value)
        {
            if (value == null)
            {
                return new[] { "Value is null." };
            }

            var result = new List<string>();

            //The following check has to be removed until the iOS bug that returns TEKs regardless of age is fixed.
            //var earliestAcceptedDateFromDevices = _DateTimeProvider.Snapshot.Date - TimeSpan.FromDays(_Config.MaxAgeDays);
            //var rollingStartMin = Math.Max(_Config.RollingStartNumberMin, earliestAcceptedDateFromDevices.ToRollingStartNumber());

            var rollingStartMin = _config.RollingStartNumberMin;
            var rollingStartToday = _dateTimeProvider.Snapshot.Date.ToRollingStartNumber();

            // This checks no keys come in with RollingStartNumbers from before 2020-07-01 (date of release of GA SDKs)
            // Invalidate entire upload if it contains a key with such a RollingStartNumber.
            if (value.RollingStartNumber < rollingStartMin)
            {
                result.Add($"RollingStartNumber out of range - {value.RollingStartNumber}.");
            }

            if (string.IsNullOrEmpty(value.KeyData))
            {
                result.Add("KeyData is empty.");
            }

            var buffer = new Span<byte>(new byte[UniversalConstants.DailyKeyDataByteCount]);
            if (!Convert.TryFromBase64String(value.KeyData, buffer, out var count))
            {
                result.Add("KeyData is not valid base64.");
            }

            if (UniversalConstants.DailyKeyDataByteCount != count)
            {
                result.Add($"KeyData length incorrect - {count}.");
            }

            return result.ToArray();
        }
    }
}
