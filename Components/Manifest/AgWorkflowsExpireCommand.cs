// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class AgWorkflowsExpireCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbConfig;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IAgConfig _AgConfig;

        public AgWorkflowsExpireCommand(IDbContextProvider<ExposureContentDbContext>config, IUtcDateTimeProvider utcDateTimeProvider, IAgConfig agConfig)
        {
            _DbConfig = config;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _AgConfig = agConfig;
        }

        public async Task Execute()
        {
            var now = _UtcDateTimeProvider.Now();

            var hushNow = _DbConfig.Current.Set<KeysFirstTekReleaseWorkflowEntity>()
                .Where(x => x.Created < now - TimeSpan.FromDays(_AgConfig.WorkflowSecretLifetimeDays))
                .ToArray();

            await _DbConfig.Current.BulkDeleteAsync(hushNow);
        }
    }
}