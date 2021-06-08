// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class OriginBuilder
    {
        private readonly Uri _baseUrl;

        public OriginBuilder(string baseUrl)
        {
            _baseUrl = new Uri(baseUrl);
        }

        public string GetOrigin()
        {
            var portAppendix = ((_baseUrl.Port != 80 && _baseUrl.Port != 443) ? ":" + _baseUrl.Port : "");
            return $"{_baseUrl.Scheme}://{_baseUrl.Host}{portAppendix}";
        }
    }
}