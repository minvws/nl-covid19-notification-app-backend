// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class OriginBuilder
    {
        private readonly Uri _BaseUrl;
        
        public OriginBuilder(string baseUrl)
        {
            _BaseUrl = new Uri(baseUrl);
        }

        public string GetOrigin()
        {
            var portAppendix = ((_BaseUrl.Port != 80 && _BaseUrl.Port != 443) ? ":" + _BaseUrl.Port : "");
            return $"{_BaseUrl.Scheme}://{_BaseUrl.Host}{portAppendix}";
        }
    }
}