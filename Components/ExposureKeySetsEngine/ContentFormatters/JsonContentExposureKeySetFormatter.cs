// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    public class JsonContentExposureKeySetFormatter : IJsonExposureKeySetFormatter
    {
        public string Build(TemporaryExposureKeyArgs[] items)
            => JsonConvert.SerializeObject(items);
    }
}