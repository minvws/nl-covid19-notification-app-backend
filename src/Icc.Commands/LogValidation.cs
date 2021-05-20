// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{

    public static class LogValidation
    {
        // Log validation error messages
        public static bool LogValidationMessages(this ILogger logger, string[] messages)
        {
            if (messages?.Any(string.IsNullOrWhiteSpace) ?? true)
            {
                throw new ArgumentException(nameof(messages));
            }

            if (messages.Length == 0)
            {
                return false;
            }

            logger.WriteLogValidationError(string.Join(Environment.NewLine, messages));

            return true;
        }

        // Log filtering error messages
        public static void LogFilterMessages(this ILogger logger, string[] messages)
        {
            if (messages?.Any(string.IsNullOrWhiteSpace) ?? true)
            {
                throw new ArgumentException(nameof(messages));
            }

            if (messages.Length == 0)
            {
                return;
            }

            logger.WriteLogValidationInfo(string.Join(Environment.NewLine, messages));
        }
    }
}