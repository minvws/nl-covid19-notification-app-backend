// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

public class HsmSignerRequestModel
{
    public string Algorithm { get; set; }
    public string Cert { get; set; }
    public string Hash { get; set; }
    public bool TimeStamp { get; set; }
}
