// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    [TestClass]
    public class AuthCodeServiceTests
    {
        //
        // private 
        //
        //
        // private ITheIdentityHubService _TheIdentityHubService;
        // private IOptionsMonitor<TheIdentityHubOptions> _Options;
        // private WireMockServer _Server;
        //
        // [TestInitialize]
        // public void Initialize()
        // {
        //     var logger = new TestLogger<TheIdentityHubService>();
        //     _Server = WireMockServer.Start();
        //     
        //     if (_Server.Urls.Length < 1)
        //     {
        //         Assert.Fail("WireMockServer not started correctly");
        //         return;
        //     }
        //
        //     var serviceCollection = new ServiceCollection();
        //     serviceCollection.AddCustomOptions(TheIdentityHubDefaults.AuthenticationScheme, options =>
        //     {
        //         options.TheIdentityHubUrl = new Uri(_Server.Urls[0]);
        //         options.Tenant = "ggdghornl_test";
        //         options.ClientId = "0";
        //         options.ClientSecret = "supersecret";
        //         options.CallbackPath = "/signin-identityhub";
        //         options.Backchannel = new HttpClient();
        //     });
        //     var builder = serviceCollection.BuildServiceProvider();
        //     _Options = builder.GetService<IOptionsMonitor<TheIdentityHubOptions>>();
        //
        //     _TheIdentityHubService = new TheIdentityHubService(_Options, logger);
        // }
        //
        //
    }
}