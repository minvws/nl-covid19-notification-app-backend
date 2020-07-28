// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;


namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging
{

    public static class LogValidation
    {
        public static void Log(this ILogger logger, ValidationResult vr)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (vr == null) throw new ArgumentNullException(nameof(vr));

            if (!vr.IsValid)
                logger.LogError(string.Join(Environment.NewLine, vr.Messages));
        }

        public static bool LogValidationMessages(this ILogger logger, string[] messages)
        {
            if (messages?.Any(string.IsNullOrWhiteSpace) ?? true)
                throw new ArgumentException(nameof(messages));

            if (messages.Length == 0)
                return false;

            logger.LogError(string.Join(Environment.NewLine, messages));

            return true;
        }
    }
}