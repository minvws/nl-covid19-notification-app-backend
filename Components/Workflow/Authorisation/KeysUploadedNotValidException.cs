// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    //TODO Replace with validator that returns bool (+ messages if needed for UI)
    public class KeysUploadedNotValidException : Exception
    {
        public KeyReleaseWorkflowState KeyReleaseWorkflowState { get; } //TODO can be null????

        public KeysUploadedNotValidException()
        {
        }

        public KeysUploadedNotValidException(KeyReleaseWorkflowState state)
        {
            KeyReleaseWorkflowState = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}