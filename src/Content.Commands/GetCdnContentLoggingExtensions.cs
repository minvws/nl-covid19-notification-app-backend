// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class GetCdnContentLoggingExtensions
    {
        private const string Name = "HttpGetCdnContent";
        private const int Base = LoggingCodex.GetCdnContent;

        private const int InvalidType = Base + 1;
        private const int InvalidId = Base + 2;
        private const int HeaderMissing = Base + 3;
        private const int NotFound = Base + 4;
        private const int EtagFound = Base + 5;

        private readonly ILogger _Logger;

        public GetCdnContentLoggingExtensions(ILogger<GetCdnContentLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteInvalidType(string cdnId)
        {
            _Logger.LogError("[{name}/{id}] Invalid generic content type - {Id}.",
                Name, InvalidType,
                cdnId);
        }

        public void WriteInvalidId(string cdnId)
        {
            _Logger.LogError("[{name}/{id}] Invalid content id - {Id}.",
                Name, InvalidId,
                cdnId);
        }

        public void WriteHeaderMissing()
        {
            _Logger.LogDebug("[{name}/{id}] Required request header missing - if-none-match.",
                Name, HeaderMissing);
        }

        public void WriteNotFound(string cdnId)
        {
            _Logger.LogError("[{name}/{id}] Content not found - {Id}.",
                Name, NotFound,
                cdnId);
        }

        public void WriteEtagFound(string cdnId)
        {
            _Logger.LogWarning("[{name}/{id}] Matching etag found, responding with 304 - {Id}.",
                Name, EtagFound,
                cdnId);
        }
    }
}
