// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ResponsePadding
{
    public static class LoggingExtensionsResponsePadding
    {

        private const string Name = "PostDecoyPadding";
        private const int Base = LoggingCodex.ResponsePadding;

        private const int NoPaddingNeeded = Base + 1;
        private const int ResponsePaddingLength = Base + 2;
        private const int ResponsePaddingContent = Base + 3;
        private const int PaddingAdded = Base + 4;

        public static void WriteNoPaddingNeeded(this ILogger logger, int resultLength, int minimumLength)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] No padding needed as response length of {Length} is greater than the minimum of {MinimumLengthInBytes}.",
                Name, NoPaddingNeeded,
                resultLength, minimumLength);
        }

        public static void WriteLengthOfResponsePadding(this ILogger logger, int paddingLength)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Length of response padding:{PaddingLength}.",
            Name, ResponsePaddingLength,
                paddingLength);
        }

        public static void WritePaddingContent(this ILogger logger, string? padding)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Response padding:{Padding}",
                Name, ResponsePaddingContent,
                padding);
        }

        public static void WritePaddingAdded(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Added padding to the response.",
                Name, PaddingAdded);
        }

    }
}
