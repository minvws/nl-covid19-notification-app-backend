// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class DynamicContentExpiryStrategy : IContentExpiryStrategy
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public DynamicContentExpiryStrategy(IUtcDateTimeProvider dateTimeProvider)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public ExpiryStatus Apply(DateTime created, IHeaderDictionary headers)
        {
            if(headers == null) throw new ArgumentNullException(nameof(headers));

            var expiry = created.AddDays(14);
            var secondsToLive = (expiry - _DateTimeProvider.Now()).TotalSeconds;

            if (secondsToLive <= 0) return ExpiryStatus.Expired;

            headers.Add("cache-control", $"public, immutable, max-age={secondsToLive}, s-maxage={secondsToLive}");

            return ExpiryStatus.Active;
        }
    }
}