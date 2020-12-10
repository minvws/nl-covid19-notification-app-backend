// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests
{
    public class AuthorisationArgsValidatorTests
    {
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("22233")]
        [InlineData("222-222")]
        [InlineData("2223335")]
        [InlineData("ABCF23")]
        [InlineData("BCAD10")]
        [InlineData("U6GCQE")]
        [InlineData("U6GC1E")]
        [InlineData("U1GCCE")]
        [Theory]
        public void Invalid(string labConfirmationId)
        {
            // Assemble
            var dtp = new StandardUtcDateTimeProvider();
            var validator = new AuthorisationArgsValidator(new LabConfirmationIdService(new StandardRandomNumberGenerator()), dtp);
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = dtp.Snapshot};

            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.NotEmpty(result);
        }

        [InlineData("468RG8")]
        [InlineData("U6GCQF")]
        [InlineData("FU7VXB")]
        [InlineData("TVG6SU")]
        [Theory]
        public void Valid(string labConfirmationId)
        {
            // Assemble
            var dtp = new StandardUtcDateTimeProvider();
            var validator = new AuthorisationArgsValidator(new LabConfirmationIdService(new StandardRandomNumberGenerator()), dtp);
            var args = new AuthorisationArgs { LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = dtp.Snapshot };
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.Empty(result);
        }
    }
}