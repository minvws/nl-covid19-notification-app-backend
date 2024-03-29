// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests
{
    public static class TestSignerHelpers
    {
        public static IHsmSignerService CreateHsmSignerService()
        {
            var dummyContent = Encoding.ASCII.GetBytes("Signature intentionally left empty");

            var hsmServiceMock = new Mock<IHsmSignerService>();
            hsmServiceMock
                .Setup(x => x.GetNlCmsSignatureAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(dummyContent);

            return hsmServiceMock.Object;
        }
    }
}
