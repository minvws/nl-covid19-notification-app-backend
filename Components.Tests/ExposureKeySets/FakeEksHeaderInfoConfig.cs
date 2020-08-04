// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeEksHeaderInfoConfig : IEksHeaderInfoConfig
    {
        public string AppBundleId => "nl.rijksoverheid.en";
        public string VerificationKeyId => "ServerNL";
        public string VerificationKeyVersion => "v1";
    }
}