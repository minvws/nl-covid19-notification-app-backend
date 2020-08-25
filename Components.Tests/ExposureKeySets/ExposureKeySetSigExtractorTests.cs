// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [TestClass]
    public class ExposureKeySetSigExtractorTests
    {

        [TestMethod]
        public void Yank()
        {
            const string eks = "1a082a5e05cd791ef7fbabdf3b653d3d9363d1dfb791d026e81db519500b090c";

            var folder = Path.GetDirectoryName(NCrunch.Framework.NCrunchEnvironment.GetOriginalProjectPath());
            var filename = Path.Combine(folder, eks);
            var bytes = new SigReader().Read(filename);
            using var output = File.Create(@"D:\sig.bin", 1024, FileOptions.None);
            output.Write(bytes);
        }
    }
}