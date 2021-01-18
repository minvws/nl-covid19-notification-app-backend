// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1
{
    public class EksBuilderV1LoggingExtensions
    {
        private const string Name = "EksBuilderV1";
        private const int Base = LoggingCodex.EksBuilderV1;

        private const int NlSig = Base + 1;
        private const int GaenSig = Base + 2;

        private readonly ILogger _Logger;

        public EksBuilderV1LoggingExtensions(ILogger<EksBuilderV1LoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void WriteNlSig(byte[]? sig)
        {
            if (sig == null)
            {
                throw new ArgumentNullException(nameof(sig));
            }

            _Logger.LogDebug("[{name}/{id}] NL Sig: {NlSig}.",
                Name, NlSig,
                Convert.ToBase64String(sig));
        }

        public void WriteGaenSig(byte[]? sig)
        {
            if (sig == null)
            {
                throw new ArgumentNullException(nameof(sig));
            }

            _Logger.LogDebug("[{name}/{id}] GAEN Sig: {GaenSig}.",
                Name, GaenSig,
                Convert.ToBase64String(sig));
        }
    }
}