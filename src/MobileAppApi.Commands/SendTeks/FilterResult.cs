// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class FilterResult<T> where T:class
    {
        public FilterResult(T[] valid, string[] messages)
        {
            Items = valid ?? throw new ArgumentNullException(nameof(valid));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public T[] Items { get; }
        public string[] Messages { get; }
    }
}