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
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ILogger<TestJwtGeneratorService> _logger;

        public TestJwtGeneratorService(IJwtService jwtService, IUtcDateTimeProvider dateTimeProvider,
            ILogger<TestJwtGeneratorService> logger)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _logger.WriteTestJwtConstructed();
            var testJwtData = new Dictionary<string, object> { { "access_token", "test_access_token" }, { "id", "0" } };

            var expiry = new StandardUtcDateTimeProvider().Now().AddDays(7).ToUnixTimeU64();

            _logger.WriteGeneratedToken(jwtService.Generate(expiry, testJwtData));
        }
    }
}
