// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Authorisation
{
    [TestClass]
    public class AuthorisationArgsValidatorTests
    {
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("22233")]
        [DataRow("222-222")]
        [DataRow("2223335")]
        [DataRow("ABCF23")]
        [DataRow("BCAD10")]
        [DataRow("U6GCQE")]
        [DataRow("U6GC1E")]
        [DataRow("U1GCCE")]
        [TestMethod]
        public void Invalid(string labConfirmationId)
        {
            // Assemble
            var dtp = new StandardUtcDateTimeProvider();
            var validator = new AuthorisationArgsValidator(new LabConfirmationIdService(new StandardRandomNumberGenerator()), dtp);
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = dtp.Snapshot};

            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsTrue(result.Any());
        }

        [DataRow("468RG8")]
        [DataRow("U6GCQF")]
        [DataRow("FU7VXB")]
        [DataRow("TVG6SU")]
        [TestMethod]
        public void Valid(string labConfirmationId)
        {
            // Assemble
            var dtp = new StandardUtcDateTimeProvider();
            var validator = new AuthorisationArgsValidator(new LabConfirmationIdService(new StandardRandomNumberGenerator()), dtp);
            var args = new AuthorisationArgs { LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = dtp.Snapshot };
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsTrue(!result.Any());
        }
    }
}