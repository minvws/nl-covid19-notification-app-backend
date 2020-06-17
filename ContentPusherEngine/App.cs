// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class App
    {
        private readonly ILogger<App> _Logger;

        public App(ILogger<App> logger)
        {
            _Logger = logger;
        }

        public async Task Run()
        {
            _Logger.LogInformation("Running...");
            await Task.Delay(1000);
            _Logger.LogInformation("Completed...");
        }
    }
}
