// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1
{
    public class ExposureKeySetSignatureContentArgs
    {
        public SignatureInfoArgs SignatureInfo { get; set; }
        public byte[] Signature { get; set; }
        public int BatchSize { get; set; }
        public int BatchNum { get; set; }
    }
}