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

        /// <summary>
        /// Assumes future TEKs already given mercy.
        /// Filters out ones too old that would not get into the EKS
        /// </summary>
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
            var result = values.Where(x => x.RollingStartNumber >= lowerLimit).ToArray();

            var messages = values.Except(result)
                .Select(x => $"TEKs from before {lowerInclusiveDate:yyyy-MM-dd} cannot be published - RSN:{x.RollingStartNumber} = {x.RollingStartNumber.FromRollingStartNumber():yyyy-MM-dd}.")
                .ToArray();

            return new FilterResult<Tek>(result, messages);
        }
    }
}
