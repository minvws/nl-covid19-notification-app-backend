// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers
{
    public interface ICertificateLocationConfig
    {
        public string Path { get; }
        public string Password { get; }
    }

    public class HardCodedCertificateLocationConfig : ICertificateLocationConfig
    {
        public HardCodedCertificateLocationConfig(string path, string password)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public string Path { get; }
        public string Password { get; }
    }
}