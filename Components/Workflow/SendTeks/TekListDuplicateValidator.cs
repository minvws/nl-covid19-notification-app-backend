// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class TekListDuplicateValidator
    {
        /// <summary>
        /// Individual keys assumed previously valid.
        /// Checking for time-based overlaps
        /// TODO is previous key altered? E.g. is Period of previous key shortened?
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public string[] Validate(Tek[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            if (values.Length < 2)
                return new string[0];

            var ordered = values.OrderBy(x => x.RollingStartNumber).ToArray();

            for (var i = 0; i < ordered.Length - 1; i++)
            {
                if (ordered[i].SameTime(ordered[i + 1]))
                    return new[] { $"There is at least one duplicate - RollingStartNumber:{ordered[i].RollingStartNumber} RollingPeriod:{ordered[i].RollingPeriod}" };
            }

            return new string[0];
        }
    }
}