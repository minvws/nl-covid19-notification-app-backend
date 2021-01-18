// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class ResponsePaddingLoggingExtensions
    {
        private const string Name = "ResponsePadding";
        private const int Base = LoggingCodex.ResponsePadding;

        private const int NoPaddingNeeded = Base + 1;
        private const int ResponsePaddingLength = Base + 2;
        private const int ResponsePaddingContent = Base + 3;
        private const int PaddingAdded = Base + 4;

        private readonly ILogger _Logger;

        public ResponsePaddingLoggingExtensions(ILogger<ResponsePaddingLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteNoPaddingNeeded(int resultLength, int minimumLength)
        {
            _Logger.LogInformation("[{name}/{id}] No padding needed as response length of {Length} is greater than the minimum of {MinimumLengthInBytes}.",
                Name, NoPaddingNeeded,
                resultLength, minimumLength);
        }

        public void WriteLengthOfResponsePadding(int paddingLength)
        {
            _Logger.LogInformation("[{name}/{id}] Length of response padding:{PaddingLength}.",
                Name, ResponsePaddingLength,
                paddingLength);
        }

        public void WritePaddingContent(string? padding)
        {
            _Logger.LogDebug("[{name}/{id}] Response padding:{Padding}",
                Name, ResponsePaddingContent,
                padding);
        }

        public void WritePaddingAdded()
        {
            _Logger.LogInformation("[{name}/{id}] Added padding to the response.",
                Name, PaddingAdded);
        }

    }
}
