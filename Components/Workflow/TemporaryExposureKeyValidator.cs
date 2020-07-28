// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Amqp.Serialization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidator : ITemporaryExposureKeyValidator
    {
        private readonly ITemporaryExposureKeyValidatorConfig _Config;
        //private readonly IUtcDateTimeProvider _DateTimeProvider;

        public TemporaryExposureKeyValidator(ITemporaryExposureKeyValidatorConfig config /*, IUtcDateTimeProvider dateTimeProvider*/)
        {
            _Config = config;
            //_DateTimeProvider = dateTimeProvider;
        }

        public string[] Valid(PostTeksItemArgs value)
        {
            if (value == null)
                return new [] {"Value is null."};

            var result = new List<string>();

            //TODO define minimum
            //TODO move to EKS Engine?
            //var now = _DateTimeProvider.Now().Date;
            //var rollingStartNumberMin = now.AddDays(-14).ToRollingPeriodNumber(); //TODO setting
            //var rollingStartNumberMax = now.AddDays(1).ToRollingPeriodNumber(); //TODO setting
            //if (rollingStartNumberMin > value.RollingStartNumber || value.RollingStartNumber > rollingStartNumberMax)
            //    result.Add($"RollingStart out of range - {value.RollingStartNumber}.");

            if (_Config.RollingPeriodMin > value.RollingPeriod || value.RollingPeriod > _Config.RollingPeriodMax)
                result.Add($"RollingPeriod out of range - {value.RollingPeriod}.");

            if (string.IsNullOrEmpty(value.KeyData))
                result.Add("KeyData is empty.");

            var buffer = new Span<byte>(new byte[_Config.DailyKeyByteCount]);
            if (!Convert.TryFromBase64String(value.KeyData, buffer, out int count))
                result.Add("KeyData is not valid base64.");

            if (_Config.DailyKeyByteCount != count)
                result.Add($"KeyData length incorrect - {count}.");

            return result.ToArray();
        }
    }
}