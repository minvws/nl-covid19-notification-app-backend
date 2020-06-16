// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class KeysLastSecretValidator : IKeysLastSecretValidator
    {
        private readonly IKeysLastSecretConfig _Config;

        public KeysLastSecretValidator(IKeysLastSecretConfig config)
        {
            _Config = config;
        }

        public bool Valid(KeysLastSecretArgs args)
        {
            if (string.IsNullOrWhiteSpace(args?.ConfirmationKey))
                return false;

            var _ = new Span<byte>(new byte[_Config.ByteCount]);
            if (!Convert.TryFromBase64String(args.ConfirmationKey, _, out var length))
                return false;

            if (length != _Config.ByteCount)
                return false;

            //TODO Anything else?

            return true;
        }
    }
}