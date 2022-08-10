// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

public interface IHsmSignerConfig
{
    public string BaseAddress { get; }
    public string CmsJwt { get; }
    public string GaenJwt { get; }
    public string CmsPublicCertificateChain { get; }
    public string GaenPublicCertificate { get; }
}
