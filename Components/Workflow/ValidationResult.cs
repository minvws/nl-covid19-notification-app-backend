// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class ValidationResult
    {
        public ValidationResult(params string[] messages)
        {
            if (messages == null || messages.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException(nameof(messages));

            Messages = messages;
        }

        public static readonly ValidationResult Valid = new ValidationResult();

        public bool IsValid => Messages.Length == 0;
        public string[] Messages { get; }
    }
}