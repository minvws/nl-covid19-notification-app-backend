// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class LabConfirmationIdServiceTests
    {
        // TODO test looks like it's in the wrong class and looks old / wrong?
        // TODO agreed - move to Domain.Tests
        [InlineData(1)]
        [InlineData(256)]
        [InlineData(3030)]
        [Theory]
        public void GenerateToken(int length)
        {
            var random = new LabConfirmationIdService(new StandardRandomNumberGenerator());
            var result = random.Next();
            Assert.True(result.Length == 6);

            Assert.True(random.Validate(result).Length == 0);

        }
    }
}
