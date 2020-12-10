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
        private readonly IContentSigner _ContentSigner;

        public ZippedSignedContentFormatter(IContentSigner contentSigner)
        {
            _ContentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
        }

        public async Task<byte[]> SignedContentPacketAsync(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var signature = _ContentSigner.GetSignature(content);
            return await new ZippedContentBuilder().BuildStandardAsync(content, signature);
        }
    }
}