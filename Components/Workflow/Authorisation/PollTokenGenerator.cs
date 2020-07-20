// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class PollTokenGenerator
    {
        private readonly JwtService _JwtService;

        public PollTokenGenerator(JwtService jwtService)
        {
            _JwtService = jwtService;
        }

        public string GenerateToken()
        {
            return _JwtService.GenerateCustomJwt(DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds(),
                new Dictionary<string, object>
                {
                    ["payload"] = Guid.NewGuid().ToString() // make polltoken unique
                });
        }

        public bool Verify(string token)
        {
            return _JwtService.IsValidJwt(token,"payload");
        }
        
    }
}