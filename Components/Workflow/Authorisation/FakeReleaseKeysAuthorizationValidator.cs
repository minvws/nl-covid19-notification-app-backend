// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeReleaseKeysAuthorizationValidator : IReleaseKeysAuthorizationValidator
    {
        public bool Valid(string token) => !string.IsNullOrEmpty(token);
    }
}