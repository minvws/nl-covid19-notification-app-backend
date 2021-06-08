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
        private readonly ITekListValidationConfig _config;
        private readonly ITemporaryExposureKeyValidator _temporaryExposureKeyValidator;

        public PostTeksArgsValidator(ITekListValidationConfig config, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _temporaryExposureKeyValidator = temporaryExposureKeyValidator ?? throw new ArgumentNullException(nameof(temporaryExposureKeyValidator));
        }

        public string[] Validate(PostTeksArgs args)
        {
            if (args == null)
            {
                return new[] { "Null value." };
            }

            if (_config.TemporaryExposureKeyCountMin > args.Keys.Length || args.Keys.Length > _config.TemporaryExposureKeyCountMax)
            {
                return new[] { $"Invalid number of keys - {args.Keys.Length}." };
            }

            var keyErrors = new List<string>();

            for (var i = 0; i < args.Keys.Length; i++)
            {
                keyErrors.AddRange(_temporaryExposureKeyValidator.Valid(args.Keys[i]).Select(x => $"Key[{i}] - {x}"));
            }

            return keyErrors.ToArray();
        }

    }
}
