// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class KeysUploadedNotValidException : Exception
    {
        public KeyReleaseWorkflowState KeyReleaseWorkflowState { get; } //TODO can be null????

        public KeysUploadedNotValidException() : base()
        {
        }

        public KeysUploadedNotValidException(KeyReleaseWorkflowState state) : base()
        {
            KeyReleaseWorkflowState = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}