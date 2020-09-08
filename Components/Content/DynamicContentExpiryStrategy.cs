// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class DynamicContentExpiryStrategy : IContentExpiryStrategy
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IHttpResponseHeaderConfig _HttpResponseHeaderConfig;

        public DynamicContentExpiryStrategy(IUtcDateTimeProvider dateTimeProvider, IHttpResponseHeaderConfig httpResponseHeaderConfig)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _HttpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
        }

        public int Calculate(DateTime created)
        {
            var expiry = created.AddSeconds(_HttpResponseHeaderConfig.EksMaxTtl);
            var ttl = (int)(expiry - _DateTimeProvider.Now()).TotalSeconds;

            return ttl < 0 ? 0 : ttl;
        }
    }
}