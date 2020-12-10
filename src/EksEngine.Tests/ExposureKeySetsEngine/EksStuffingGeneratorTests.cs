// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    public class EksStuffingGeneratorTests
    {

        [Fact]
        public void NewEksStuffingGeneratorTest()
        {
            var rng = new StandardRandomNumberGenerator(); var eksConfigMock = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfigMock.Setup(x => x.LifetimeDays).Returns(14);
            var dtp = new StandardUtcDateTimeProvider();

            var stuffer = new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), rng, dtp, eksConfigMock.Object);

            var result = stuffer.Execute(10);

            Assert.Equal(10, result.Length);
        }
    }
}