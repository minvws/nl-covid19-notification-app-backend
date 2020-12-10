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
        private readonly IJwtService _JwtService;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger<TestJwtGeneratorService> _Logger;
        
        public TestJwtGeneratorService(IJwtService jwtService, IUtcDateTimeProvider dateTimeProvider,
            ILogger<TestJwtGeneratorService> logger)
        {
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        
            _Logger.WriteTestJwtConstructed();
            var testJwtData = new Dictionary<string, object> {{"access_token", "test_access_token"}, {"id", "0"}};
        
            var expiry = new StandardUtcDateTimeProvider().Now().AddDays(7).ToUnixTimeU64();
        
            _Logger.WriteGeneratedToken(jwtService.Generate(expiry, testJwtData));
        }
    }
}