// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ZippedContentBuilderArgs
    {
        public byte[] Value { get; set; }
        public string EntryName { get; set; }
    }
}