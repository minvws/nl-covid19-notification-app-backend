// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using JWT;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Authorisation
{
    [TestClass]
    public class LabVerifyArgsValidatorTests
    {
        private LabVerifyArgsValidator validator;
        private PollTokenService _pollTokenService;
        private double claimLifetimeHours = 3;
        private IUtcDateTimeProvider _utcDateTimeProvider;

        [TestInitialize]
        public void Initialize()
        {
            var lf = new TestLogger<JwtService>();
            _utcDateTimeProvider = new StandardUtcDateTimeProvider();
            _pollTokenService =
                new PollTokenService(
                    new JwtService(
                        new HardCodedIccPortalConfig(null, "http://test.test", "test_secret123", claimLifetimeHours),
                        _utcDateTimeProvider, logger: lf), _utcDateTimeProvider);
            validator = new LabVerifyArgsValidator(_pollTokenService);
        }

        [TestMethod]
        public void Validate_checks_null()
        {
            // Assemble 
            var args = new LabVerifyArgs();
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void Validate_checks_pollToken_format_1()
        {
            // Assemble 
            var args = new LabVerifyArgs() {PollToken = "test_not_base64"};
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_checks_pollToken_format_2()
        {
            // Assemble 
            var args = new LabVerifyArgs() {PollToken = "test_not_base64.part2.part3"};
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_checks_pollToken_valid()
        {
            // Assemble 
            var token = _pollTokenService.GenerateToken();
            var args = new LabVerifyArgs() {PollToken = token};
            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsTrue(result);
        }
        
        //
        // [TestMethod]
        // public void Validate_checks_pollToken_expires()
        // {
        //     // Assemble 
        //     var token = _pollTokenService.GenerateToken();
        //     var args = new LabVerifyArgs() {PollToken = token};
        //
        //     // Act
        //     // var mock = new Mock<IDateTime>();
        //     // mock.Setup(fake => fake.Now)
        //     // .Returns(new DateTime(2000, 1, 1));
        //     // TODO Add Moq?
        //     var result = validator.Validate(args);
        //
        //     // Assert
        //     Assert.IsTrue(result);
        // }

        [TestMethod]
        public void Validate_checks_pollToken_signature()
        {
            // Assemble 
            var token = _pollTokenService.GenerateToken();

            token = token.Substring(0, token.Length - 2); // modifies end of token which is signature

            var args = new LabVerifyArgs() {PollToken = token};

            // Act
            var result = validator.Validate(args);

            // Assert
            Assert.IsFalse(result);
        }
    }
}