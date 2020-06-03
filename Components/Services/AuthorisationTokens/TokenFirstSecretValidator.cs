// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class TokenFirstSecretValidator : ITokenFirstSecretValidator
    {
        private readonly ITokenFirstSecretConfig _Config;

        public TokenFirstSecretValidator(ITokenFirstSecretConfig config)
        {
            _Config = config;
        }

        public bool Valid(TokenFirstSecretArgs args)
        {
            if (string.IsNullOrWhiteSpace(args?.Token))
                return false;

            var r = new Span<byte>();
            if (!Convert.TryFromBase64Chars(args.Token, r, out _))
                return false;

            if (r.Length != _Config.ByteCount)
                return false;

            //TODO Anything else?

            return true;
        }
    }
}