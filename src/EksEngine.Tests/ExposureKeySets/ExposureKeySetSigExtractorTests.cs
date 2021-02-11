// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    public class ExposureKeySetSigExtractorTests
    {

        [Fact]
        public void Yank()
        {
            const string eks = "1a082a5e05cd791ef7fbabdf3b653d3d9363d1dfb791d026e81db519500b090c";

            var folder = Path.GetDirectoryName("~");
            var filename = Path.Combine(folder, eks);
            var bytes = new EksParser().ReadGaenSig(filename);
            Assert.True(bytes.Length > 0);
            using var output = File.Create(@"sig.bin", 1024, FileOptions.None);
            output.Write(bytes);
        }
    }
}