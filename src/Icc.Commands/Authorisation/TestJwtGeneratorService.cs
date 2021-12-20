// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class TestJwtGeneratorService
    {
        private readonly IJwtService _jwtService;
        private readonly ILogger<TestJwtGeneratorService> _logger;

        public TestJwtGeneratorService(IJwtService jwtService, ILogger<TestJwtGeneratorService> logger)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("TestJwtGeneratorService Singleton constructed, generating test JWT now....");
            var testJwtData = new Dictionary<string, object> { { "access_token", "test_access_token" }, { "id", "0" } };

            var expiry = new StandardUtcDateTimeProvider().Now().AddDays(7).ToUnixTimeU64();

            var token = _jwtService.Generate(expiry, testJwtData);
            _logger.LogInformation("{Token}.", token);
        }
    }
}
