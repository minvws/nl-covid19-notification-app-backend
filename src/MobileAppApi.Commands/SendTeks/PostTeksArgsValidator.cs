// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class PostTeksArgsValidator : IPostTeksValidator
    {
        private readonly ITekListValidationConfig _Config;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public PostTeksArgsValidator(ITekListValidationConfig config, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator ?? throw new ArgumentNullException(nameof(temporaryExposureKeyValidator));
        }

        public string[] Validate(PostTeksArgs args)
        {
            if (args == null)
                return new[] {"Null value."};

            if (_Config.TemporaryExposureKeyCountMin > args.Keys.Length || args.Keys.Length > _Config.TemporaryExposureKeyCountMax)
                return new[] {$"Invalid number of keys - {args.Keys.Length}."};

            var keyErrors = new List<string>();

            for (var i = 0; i < args.Keys.Length; i++)
            {
                keyErrors.AddRange(_TemporaryExposureKeyValidator.Valid(args.Keys[i]).Select(x => $"Key[{i}] - {x}"));
            }

            return keyErrors.ToArray();
        }

    }
}