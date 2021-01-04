// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class TekListDuplicateKeyDataValidator
    {
        public string[] Validate(Tek[] values)
        {
            var comparer = new ByteArrayEqualityComparer();
            var distinctCount = values.Select(x => x.KeyData).Distinct(comparer).Count();
            if (values.Length != distinctCount)
                return new[] { $"KeyData duplicate found - Count:{values.Length - distinctCount}." };

            return new string[0];
        }
    }
}