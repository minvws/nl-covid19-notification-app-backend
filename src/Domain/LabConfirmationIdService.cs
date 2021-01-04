// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class LabConfirmationIdService : ILabConfirmationIdService
    {
        private readonly IRandomNumberGenerator _Rng;

        private const int Length = 6;
        private const string PermittedCharacters = "BCFGJLQRSTUVXYZ23456789";

        public LabConfirmationIdService(IRandomNumberGenerator rng)
        {
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public string Next()
        {
            var token = new StringBuilder(Length);
            for (var i = 0; i < Length; i++)
            {
                var index = _Rng.Next(0, PermittedCharacters.Length - 1);
                token.Append(PermittedCharacters[index]);
            }
            return token.ToString();
        }

        public string[] Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new[] { "Value is null or empty." };

            if (value.Length != Length)
                return new[] { "Value has incorrect length." };

            if (value.Any(x => !PermittedCharacters.Contains(x)))
                return new[] { "Value contains invalid character." };

            return new string[0];
        }
    }
}