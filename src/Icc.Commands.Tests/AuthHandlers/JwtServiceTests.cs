// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components.Tests.AuthHandlers
{
    //ncrunch: no coverage start 

    public class JwtServiceTests
    {
        private JwtService _JwtService;
        private double _ClaimLifetimeHours = 1.0;
        private IUtcDateTimeProvider _UtcDateTimeProvider;

        public JwtServiceTests()
        {
            var lf = new TestLogger<JwtService>();
            _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
            var mock = new Mock<IccPortalConfig>();
            //new HardCodedIccPortalConfig(null, "http://test.test", "test_secret123", _ClaimLifetimeHours, true),
            _JwtService =
                new JwtService(mock.Object,
                    _UtcDateTimeProvider, lf);
        }

        [Fact]
        public void NoUnitTestsYet()
        {
            throw new Exception("Tests not implemented yet");
        }
        
        
        // Test Custom generate method
        // - exp values -1 -- max. time
        // - claims empty and filled with different types
        // - is exp handled valid?

        // Test Default Claimbased Generate method
        // - can handle empty claims?
        // - is exp claimlifetimehours valid?
        // 


        // TryDecode
        // Check PollToken validation
    }
}