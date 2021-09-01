// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace Core.E2ETests
{
    public class TestBase
    {
        public EndpointConfig Config { get; }

        public TestBase()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()  // <== this is important
                .Build();

            Config = new EndpointConfig(configurationRoot);
        }
    }
}
