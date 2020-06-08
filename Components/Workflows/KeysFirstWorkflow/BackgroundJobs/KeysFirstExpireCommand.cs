// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.BackgroundJobs
{
    public class KeysFirstExpireCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbConfig;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IGaenContentConfig _GaenContentConfig;

        public KeysFirstExpireCommand(IDbContextProvider<ExposureContentDbContext>config, IUtcDateTimeProvider utcDateTimeProvider, IGaenContentConfig gaenContentConfig)
        {
            _DbConfig = config;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _GaenContentConfig = gaenContentConfig;
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();

            var hushNow = _DbConfig.Current.Set<KeysFirstTeksWorkflowEntity>()
                .Where(x => x.Created < now - TimeSpan.FromDays(_GaenContentConfig.KeysLastSecretLifetimeDays))
                .ToArray();

            await _DbConfig.Current.BulkDeleteAsync(hushNow);
        }
    }
}