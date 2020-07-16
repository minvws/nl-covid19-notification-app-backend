// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions
{
    public class LabVerifyKeysEmptyException : Exception
    {
        public readonly string? FreshPollToken;

        public LabVerifyKeysEmptyException() : base()
        {
        }

        public LabVerifyKeysEmptyException(string? freshPollToken) : base()
        {
            FreshPollToken = freshPollToken;
        }
    }
}