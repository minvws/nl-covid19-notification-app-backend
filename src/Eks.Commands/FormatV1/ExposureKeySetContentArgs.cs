// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1
{
    public class ExposureKeySetContentArgs
    {
        public string Header { get; set; }
        public string Region { get; set; }
        public int BatchNum { get; set; }
        public int BatchSize { get; set; }
        public ulong StartTimestamp { get; set; }
        public ulong EndTimestamp { get; set; }
        public SignatureInfoArgs[] SignatureInfos { get; set; }
        public TemporaryExposureKeyArgs[] Keys { get; set; }
    }
}