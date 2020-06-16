// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.BackgroundJobs
{
    public class KeysFirstExpireCommand
    {
        private readonly ExposureContentDbContext _Context;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IGaenContentConfig _GaenContentConfig;

        public KeysFirstExpireCommand(ExposureContentDbContext context, IUtcDateTimeProvider utcDateTimeProvider, IGaenContentConfig gaenContentConfig)
        {
            _Context = context;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _GaenContentConfig = gaenContentConfig;
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();

            var hushNow = _Context.Set<KeyReleaseWorkflowState>()
                .Where(x => x.Created < now - TimeSpan.FromDays(_GaenContentConfig.KeysLastSecretLifetimeDays))
                .ToArray();

            await _Context.BulkDeleteAsync(hushNow);
        }
    }
}