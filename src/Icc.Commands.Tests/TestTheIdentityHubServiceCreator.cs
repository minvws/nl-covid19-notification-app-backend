// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using TheIdentityHub.AspNetCore.Authentication;
using WireMock.Server;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests
{
    public static class TestTheIdentityHubServiceCreator
    {
        public static TheIdentityHubService CreateInstance(WireMockServer Server)
        {
            var logger = new TestLogger<TheIdentityHubService>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCustomOptions(TheIdentityHubDefaults.AuthenticationScheme, options =>
            {
                options.TheIdentityHubUrl = new Uri(Server.Urls[0]);
                options.Tenant = "ggdghornl_test";
                options.ClientId = "0";
                options.ClientSecret = "supersecret";
                options.CallbackPath = "/signin-identityhub";
                options.Backchannel = new HttpClient();
            });
            var builder = serviceCollection.BuildServiceProvider();
            var options = builder.GetService<IOptionsMonitor<TheIdentityHubOptions>>();

            return new TheIdentityHubService(options, logger);
        }
    }
}