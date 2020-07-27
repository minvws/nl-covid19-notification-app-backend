// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Authorisation
{
    [TestClass]
    public class LabVerifyArgsValidatorTests
    {
        [TestMethod]
        public void Validate_checks_null()
        {
            // Assemble
            // var validator = new LabVerifyArgsValidator(new PollTokenService(new JwtService(), ));
            //
            // // Act
            // var result = validator.Validate(args);
            //
            // // Assert
            // Assert.IsFalse(result);
        }
        
        
        
    }
}