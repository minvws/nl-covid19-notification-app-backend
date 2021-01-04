// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    internal static class ResourcesHook
    {
        public static Stream? GetManifestResourceStream(string path)
        {
            var a = typeof(ResourcesHook).Assembly;
            var resPath = $"{typeof(ResourcesHook).Namespace}.{path}";
            return a.GetManifestResourceStream(resPath);
        }
    }
}
