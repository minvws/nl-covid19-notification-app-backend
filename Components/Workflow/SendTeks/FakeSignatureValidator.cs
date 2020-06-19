// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeSignatureValidator : ISignatureValidator
    {
        public bool Valid(byte[] signature, KeyReleaseWorkflowState workflow, byte[] data) => signature != null && workflow != null && data != null;
    }
}