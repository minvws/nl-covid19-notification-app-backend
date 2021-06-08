// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class TekReleaseWorkflowTime : IWorkflowTime
    {
        private readonly IWorkflowConfig _workflowConfig;

        public TekReleaseWorkflowTime(IWorkflowConfig workflowConfig)
        {
            _workflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
        }

        public DateTime Expiry(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Must be UTC.");
            }

            var result = utcNow.ToLocalTime().Date + TimeSpan.FromMinutes(_workflowConfig.TimeToLiveMinutes + _workflowConfig.PermittedMobileDeviceClockErrorMinutes);
            return result.ToUniversalTime();
        }

        /// <summary>
        /// Aka Validity
        /// </summary>
        public long TimeToLiveSeconds(DateTime utcNow, DateTime utcExpiry)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Must be UTC.", nameof(utcNow));
            }

            if (utcExpiry.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Must be UTC.", nameof(utcExpiry));
            }

            if (utcNow > utcExpiry)
            {
                throw new ArgumentException("utcNow > utcExpiry.");
            }

            return Convert.ToInt64(Math.Floor((utcExpiry - utcNow).TotalSeconds - _workflowConfig.PermittedMobileDeviceClockErrorMinutes));
        }
    }
}