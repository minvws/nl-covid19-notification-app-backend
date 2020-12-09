// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests
{
    public static class AssemblyExtensions
    {
        public static Stream GetEmbeddedResourceStream(this Assembly assembly, string path)
        {
            var ns = typeof(AssemblyExtensions).Namespace;
            var resourcePath = $"{ns}.{path}";
            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}