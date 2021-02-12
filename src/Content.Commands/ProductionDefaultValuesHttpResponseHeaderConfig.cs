// // Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// // Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// // SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ProductionDefaultValuesHttpResponseHeaderConfig : IHttpResponseHeaderConfig
    {
        public string ManifestCacheControl => "s-maxage=30";
        public string ImmutableContentCacheControl => "immutable";
    }
}