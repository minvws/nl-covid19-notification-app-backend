// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Authorisation
{
    [TestClass]
    public class AuthorisationArgsValidatorTests
    {
        
        [TestMethod]
        public void Validate_checks_null(AuthorisationArgs args)
        {
            // Assemble
            var validator = new AuthorisationArgsValidator();

            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }

        [DataRow("")]
        [DataRow(" ")]
        [TestMethod]
        public void Validate_checks_labid_empty(string labConfirmationID)
        {
            // Assemble
            var validator = new AuthorisationArgsValidator();
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationID, DateOfSymptomsOnset = DateTime.Now};
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }
        
        [DataRow("22233")]
        [DataRow("222-222")]
        [DataRow("2223335")]
        [TestMethod]
        public void Validate_checks_labid_length(string labConfirmationId)
        {
            // Assemble
            var validator = new AuthorisationArgsValidator();
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = DateTime.Now};
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }
        
        [DataRow("ABCF23")]
        [DataRow("BCAD10")]
        [DataRow("U6GCQE")]
        [DataRow("U6GC1E")]
        [DataRow("U1GCCE")]
        [TestMethod]
        public void Validate_checks_labid_regex_invalid(string labConfirmationId)
        {
            // Assemble
            var validator = new AuthorisationArgsValidator();
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = DateTime.Now};
            
            // Act
            var result = validator.Validate(args); // "^[BCFGJLQRSTUVXYZ23456789]*$"
        
            // Assert
            Assert.IsFalse(result);
        }
        
        
        [DataRow("468RG8")]
        [DataRow("U6GCQF")]
        [DataRow("FU7VXB")]
        [DataRow("TVG6SU")]
        [TestMethod]
        public void Validate_checks_labid_regex_valid(string labConfirmationId)
        {
            // Assemble
            var validator = new AuthorisationArgsValidator();
            var args = new AuthorisationArgs {LabConfirmationId = labConfirmationId, DateOfSymptomsOnset = DateTime.Now};
            // Act
            var result = validator.Validate(args);
        
            // Assert
            Assert.IsTrue(result);
        }
        
    }
}