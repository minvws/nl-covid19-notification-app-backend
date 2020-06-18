// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class ExposureKeySetEntity
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public string CreatingJobName { get; set; }
        public int CreatingJobQualifier { get; set; }
        //public string DebugContentJson { get; set; }
        public byte[] Content { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
    }
}