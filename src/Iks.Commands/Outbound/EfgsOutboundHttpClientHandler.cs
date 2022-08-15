// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class EfgsOutboundHttpClientHandler : HttpClientHandler
    {
        public EfgsOutboundHttpClientHandler(
            IAuthenticationCertificateProvider efgsCertificateProvider,
            ICertificateConfig config)
        {
            if (efgsCertificateProvider == null)
            {
                throw new ArgumentNullException(nameof(efgsCertificateProvider));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ClientCertificates.Clear();
            ClientCertificateOptions = ClientCertificateOption.Manual;

            var clientCert = efgsCertificateProvider.GetCertificate();
            ClientCertificates.Add(clientCert);
        }
    }
}
