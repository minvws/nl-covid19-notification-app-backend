// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Authentication
{
    public static class AuthenticationHelper
    {
        public static void AddBasicAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<IBasicAuthenticationConfig, BasicAuthenticationConfig>();

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
        }
    }
}
