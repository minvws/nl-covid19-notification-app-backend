// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.BackgroundJobs
{
    
    /// <summary>
    /// TODO check conditions for expiry
    /// </summary>
    public class KeysLastSecretExpireDbCommand
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IKeysLastWorkflowConfig _KeysLastWorkflowConfig;

        public KeysLastSecretExpireDbCommand(DbContextProvider<WorkflowDbContext> dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IKeysLastWorkflowConfig tokenFirstWorkflowConfig)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
            _KeysLastWorkflowConfig = tokenFirstWorkflowConfig;
        }

        public void Execute()
        {
            var expired = _DateTimeProvider.Now() - TimeSpan.FromMinutes(_KeysLastWorkflowConfig.AuthorisationWindowDurationMinutes);

            _DbContextProvider.BeginTransaction();
            var q = _DbContextProvider.Current.KeysLastWorkflows
                .Where(x => x.State == KeysLastWorkflowState.Receiving && x.AuthorisationWindowStart < expired);

            _DbContextProvider.Current.KeysLastWorkflows.RemoveRange(q);
            _DbContextProvider.Current.SaveChanges();
            _DbContextProvider.SaveAndCommit();
        }
    }
}