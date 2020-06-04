// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    public class Sha256PublishingIdCreator : IPublishingIdCreator
    {
        private readonly IExposureKeySetSigning _Signer;

        public Sha256PublishingIdCreator(IExposureKeySetSigning signer)
        {
            _Signer = signer;
        }

        private string Create(byte[] contents)
            => Convert.ToBase64String(_Signer.GetSignature(contents));

        public string Create(ManifestEntity e)
            => Create(e.Content);

        public string Create(RiskCalculationContentEntity e)
            => Create(e.Content);

        /// <summary>
        /// TODO check GetHashCode works on byte[]
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public string Create(ExposureKeySetContentEntity e)
            => Create(e.Content);

        public string Create(ResourceBundleContentEntity e)
            => Create(e.Content);
    }
}