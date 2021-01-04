// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ContentArgs
    {
        public DateTime Release { get; set; }
        public string ContentType { get; set; }
        
        public string Json { get; set; }
    }
}
