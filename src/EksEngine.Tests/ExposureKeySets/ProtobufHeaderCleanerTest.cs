// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Xunit;
using System.IO;
using System.IO.Compression;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using FluentAssertions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    public class ProtobufHeaderCleanerTest
    {
        [Fact]
        public void RegularProtobufHeader_RemoveExcessBytes_HeaderWithoutExtraBytes()
        {
            //Arrange
            Func<ZipArchive, byte[]> sut = x => ProtobufHeaderCleaner.RemoveExcessBytes(x);

            var testFileLocation = "ProtobufHeaderTestFile.zip"; //found in Resources-folder; copied to destination after build
            var expectedLength = 70;

            //Act
            byte[] result;
            using (var testZip = new ZipArchive(File.Open(testFileLocation, FileMode.Open)))
            {
                result = sut.Invoke(testZip);
            }

            //Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(expectedLength);
        }
    }
}
