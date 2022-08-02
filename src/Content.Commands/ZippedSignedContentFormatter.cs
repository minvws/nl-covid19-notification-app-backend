// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ZippedSignedContentFormatter
    {
        private readonly HsmSignerHttpClient _httpClient;

        public ZippedSignedContentFormatter(HsmSignerHttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<byte[]> SignedContentPacketAsync(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var signature = await _httpClient.GetNlSignatureAsync(content);
            return await new ZippedContentBuilder().BuildStandardAsync(content, signature);
        }
    }
}
