// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    /// <summary>
    /// Transient
    /// </summary>
    public class PollTokenService : IPollTokenService
    {
        private const string PayloadElement = "payload";

        private readonly IJwtService _jwtService;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        public PollTokenService(IJwtService jwtService, IUtcDateTimeProvider dateTimeProvider)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string Next()
        {
            return _jwtService.Generate(
                _dateTimeProvider.Now().AddSeconds(30).ToUnixTimeU64(),
                new Dictionary<string, object>
                {
                    [PayloadElement] = Guid.NewGuid().ToString()
                });
        }

        public bool ValidateToken(string value)
        {
            return _jwtService.TryDecode(value, out _);
        }
    }
}