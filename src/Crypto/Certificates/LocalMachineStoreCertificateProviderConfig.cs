﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    public class LocalMachineStoreCertificateProviderConfig : AppSettingsReader, IThumbprintConfig
    {
        private T ThrowWhenNotFound<T>(string name) => throw new MissingConfigurationValueException(name);

        public LocalMachineStoreCertificateProviderConfig(IConfiguration config, string? prefix = null) : base(config, prefix) { }

        public string Thumbprint => GetConfigValue(nameof(Thumbprint), ThrowWhenNotFound<string>(nameof(Thumbprint)));
        public bool RootTrusted => GetConfigValue(nameof(RootTrusted), ThrowWhenNotFound<bool>(nameof(RootTrusted)));
    }
}