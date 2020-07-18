//// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
//// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
//// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.IO;
//using Microsoft.Extensions.Configuration;

//TODO dead code walking

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration
//{


//    /// <summary>
//    /// Just for command line apps
//    /// </summary>
//    public static class ConfigFromJsonFiles //TODO wire this up in the repeated code in Console Apps!
//    {
//        public static IConfigurationRoot GetConfigurationRoot(string envName)
//        {
//            if (string.IsNullOrWhiteSpace(envName))
//                    throw new ArgumentException(nameof(envName));

//            var config = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
//                .AddJsonFile("appsettings.json", false, false)
//                .AddJsonFile($"appsettings.{envName}.json", true, false)
//                //Any more?
//                .Build();
//            return config;
//        }
//    }
//}
