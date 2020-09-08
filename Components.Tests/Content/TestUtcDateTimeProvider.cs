// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class TestUtcDateTimeProvider : IUtcDateTimeProvider
    {
        private readonly DateTime _Time;

        public TestUtcDateTimeProvider(DateTime time)
        {
            _Time = time;
        }
            
        public DateTime Now()
        {
            return _Time;
        }

        public DateTime Snapshot { get; }
    }
}