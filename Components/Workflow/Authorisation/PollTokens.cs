// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class PollTokens
    {
        private const string PayloadElement = "payload";

        private readonly JwtService _JwtService;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public PollTokens(JwtService jwtService, IUtcDateTimeProvider dateTimeProvider)
        {
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string GenerateToken()
        {
            return _JwtService.Generate(
                _DateTimeProvider.Now().AddSeconds(30).ToUnixTime(),
                new Dictionary<string, object>
                {
                    [PayloadElement] = Guid.NewGuid().ToString() // make polltoken unique
                });
        }

        public bool Validate(string token)
        {
            return _JwtService.IsValid(_JwtService.Decode(token), PayloadElement);
        }
    }
}