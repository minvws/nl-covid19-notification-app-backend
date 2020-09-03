// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Auth;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc.AuthHandlers
{
    //ncrunch: no coverage start 

    public class JwtServiceTests
    {
        private JwtService _JwtService;
        private double _ClaimLifetimeHours = 3;
        private IUtcDateTimeProvider _UtcDateTimeProvider;

        public JwtServiceTests()
        {
            var lf = new TestLogger<JwtService>();
            _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
            _JwtService =
                new JwtService(
                    new HardCodedIccPortalConfig(null, "http://test.test", "test_secret123", _ClaimLifetimeHours, true),
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