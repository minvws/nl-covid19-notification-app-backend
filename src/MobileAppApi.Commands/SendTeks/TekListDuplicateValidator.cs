// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class TekListDuplicateValidator
    {
        /// <summary>
        /// Individual keys assumed previously valid.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public string[] Validate(Tek[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            if (values.Length < 2)
                return new string[0];

            return new TekListDuplicateKeyDataValidator().Validate(values);
        }
    }
}