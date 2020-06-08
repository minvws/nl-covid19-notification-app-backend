// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class GenerateKeysFirstAuthorisationToken
    {
        private readonly LuhnModNGenerator _Generator;

        public GenerateKeysFirstAuthorisationToken(ILuhnModNConfig config)
        {
            _Generator = new LuhnModNGenerator(config);
        }

        public string Next(Func<int, int> random) => _Generator.Next(random);
    }
}