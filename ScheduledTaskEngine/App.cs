// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ScheduledTaskEngine
{
    public class App
    {
        private readonly ILogger<App> _Logger;

        public App(ILogger<App> logger)
        {
            _Logger = logger;
        }

        public async Task Run(string[] args)
        {
            _Logger.LogInformation("Running...");

            var arguments = GetArguments(args);

            if (arguments.Valid)
            {
                _Logger.LogInformation($"Url: {arguments.Uri} - Method: {arguments.HttpMethod} - Authorization: {arguments.AuthorizationHeader}");
                using var client = new HttpClient();
                var requestMessage = new HttpRequestMessage(arguments.HttpMethod, arguments.Uri);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(arguments.AuthorizationHeader))
                    requestMessage.Headers.Add("Authorization", arguments.AuthorizationHeader);

                var result = await client.SendAsync(requestMessage);
                result.EnsureSuccessStatusCode();

                _Logger.LogInformation("Completed...");
            }
            else
                _Logger.LogWarning("Arguments not valid...");
        }

        private static StartArguments GetArguments(IReadOnlyList<string> args)
        {

            var result = new StartArguments();

            if (args.Count > 0)
                result.HttpMethod = new HttpMethod(args[0]);

            if (args.Count > 1 && Uri.TryCreate(args[1], UriKind.Absolute, out var uri))
                result.Uri = uri;

            if (args.Count > 2)
                result.AuthorizationHeader = args[2];

            return result;
        }
    }
}
