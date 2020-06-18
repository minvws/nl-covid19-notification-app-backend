// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Serilog.Events;
using Serilog.Sinks.EventLog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging
{
    public class EventIdProvider : IEventIdProvider
    {
        public ushort ComputeEventId(LogEvent logEvent)
        {
            //TODO: Set correct eventId
            return (ushort) logEvent.Level;
        }
    }
}
