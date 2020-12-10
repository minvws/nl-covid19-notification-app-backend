// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code
{
    public class AuthCodeGenerator : IAuthCodeGenerator
    {
        private readonly IRandomNumberGenerator _Rng;

        private const int DefaultLength = 32;
        private const string PermittedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public AuthCodeGenerator(IRandomNumberGenerator rng)
        {
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public string Next(int length)
        {
            var token = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var index = _Rng.Next(0, PermittedCharacters.Length - 1);
                token.Append(PermittedCharacters[index]);
            }
            return token.ToString();
        }
        public string Next()
        {
            return Next(DefaultLength);
        }
    }
}