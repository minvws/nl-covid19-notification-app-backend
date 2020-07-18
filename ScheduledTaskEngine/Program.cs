// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ScheduledTaskEngine
{
    internal static class Program
    {
        public static void Main(params string[] args)
        {
            new ConsoleAppRunner().Execute(args, ScheduledTaskKicker.ConfigureServices, x => x.GetService<ScheduledTaskKicker>().Run(args).GetAwaiter().GetResult());
        }
    }
}
