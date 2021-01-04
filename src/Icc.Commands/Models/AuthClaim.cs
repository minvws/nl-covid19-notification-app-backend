// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models
{
    public class AuthClaim
    {
        public string Type;
        public string Value;

        public AuthClaim(string type, string value)
        {
            Type = string.IsNullOrWhiteSpace(type) ? throw new ArgumentException(nameof(type)) : type;
            Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(nameof(value)) : value;
        }
    }
}