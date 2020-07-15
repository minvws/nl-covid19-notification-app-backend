// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    internal static class Program
    {
        public static void Main(params string[] args)
        {
            new ConsoleAppRunner().Execute(args, PusherTask.ConfigureServices, x => x.GetService<PusherTask>().PushIt().GetAwaiter().GetResult());
        }
    }
}
