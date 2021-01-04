// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public static class AssemblyExtensions
    { 
        public static T GetCustomAttribute<T>(this Assembly assembly) where T : Attribute
            => (T)assembly.GetCustomAttributes(typeof(T), false).FirstOrDefault();


        private const string Default = "Not found.";

        public static string[] Dump(this Assembly assembly)
        {
            return new []
            {
                $"Title: { assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? Default}",
                //$"Version: { assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? Default}",
                $"File Version: { assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? Default}",
                $"Informational File Version: { assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?? Default}",
                $"Product: { assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? Default}",
            };
        }
    }
}