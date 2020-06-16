// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Web;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    public class Sha256PublishingId : IPublishingId
    {
        private readonly IExposureKeySetSigning _Signer;

        public Sha256PublishingId(IExposureKeySetSigning signer)
        {
            _Signer = signer;
        }

        private string Create(byte[] contents)
            => Convert.ToBase64String(_Signer.GetSignature(contents));

        public string Create(ContentEntity e)
            => Create(e.Content);

        public string ParseUri(string uri)
            => HttpUtility.UrlDecode(uri).Replace(" ", "+");
        
    }
}