// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class TekValidPeriodFilter : ITekValidPeriodFilter
    {
        private readonly ITekValidatorConfig _config;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        public TekValidPeriodFilter(ITekValidatorConfig config, IUtcDateTimeProvider dateTimeProvider)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public FilterResult<Tek> Execute(Tek[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Any(x => x == null))
            {
                throw new ArgumentException(nameof(values));
            }

            var lowerInclusiveDate = _dateTimeProvider.Snapshot.Date - TimeSpan.FromDays(_config.MaxAgeDays);
            var lowerLimit = lowerInclusiveDate.ToRollingStartNumber();

            // Filter out TEKs that are too old, or that start in the future.
            // Also filter out TEKs that have an invalid value for RollingPeriod.
            var result = values
                .Where(x => x.RollingStartNumber >= lowerLimit
                    && x.RollingStartNumber <= _dateTimeProvider.Snapshot.ToRollingStartNumber()
                    && x.RollingPeriod >= UniversalConstants.RollingPeriodRange.Lo
                    && x.RollingPeriod <= UniversalConstants.RollingPeriodRange.Hi).ToArray();

            var messages = values.Except(result)
                .Select(x => $"TEKs with invalid time windows cannot be published. RSN too old or in future: {x.RollingStartNumber} and/or RP not in [1, 144]: {x.RollingPeriod}").ToArray();

            return new FilterResult<Tek>(result, messages);
        }
    }
}
