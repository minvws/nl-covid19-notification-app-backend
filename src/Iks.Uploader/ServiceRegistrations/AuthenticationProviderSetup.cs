// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class AuthenticationProviderSetup
    {
        public static void AuthenticationProviderRegistration(this IServiceCollection services)
        {
            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();
            services.AddTransient<IAuthenticationCertificateProvider>(x =>
                new LocalMachineStoreCertificateProvider(
                    x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>()));
        }
    }
}
