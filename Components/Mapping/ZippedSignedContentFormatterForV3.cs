// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public class ZippedSignedContentFormatterForV3
    {
        private readonly IContentSigner _ContentSigner;

        //Use in conjunction with NlSignerForV3Startup()
        public ZippedSignedContentFormatterForV3(CmsSignerEnhanced contentSigner)
        {
            _ContentSigner = contentSigner ?? throw new ArgumentNullException(nameof(contentSigner));
        }

        public async Task<byte[]> SignedContentPacket(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var signature = _ContentSigner.GetSignature(content);
            return await new ZippedContentBuilder().BuildStandard(content, signature);
        }
    }
}