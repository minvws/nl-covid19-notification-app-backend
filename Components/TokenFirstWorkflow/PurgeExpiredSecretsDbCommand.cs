// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class PurgeExpiredSecretsDbCommand
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ITokenFirstWorkflowConfig _TokenFirstWorkflowConfig;

        public PurgeExpiredSecretsDbCommand(DbContextProvider<WorkflowDbContext> dbContextProvider, IUtcDateTimeProvider dateTimeProvider, ITokenFirstWorkflowConfig tokenFirstWorkflowConfig)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
            _TokenFirstWorkflowConfig = tokenFirstWorkflowConfig;
        }

        public void Execute()
        {
            var expired = _DateTimeProvider.Now() - TimeSpan.FromDays(_TokenFirstWorkflowConfig.WorkflowTokenTtlDays);

            _DbContextProvider.BeginTransaction();
            var q = _DbContextProvider.Current.KeysLastWorkflows
                .Where(x => x.State == TokenFirstWorkflowState.Unauthorised && x.Created < expired);

            _DbContextProvider.Current.KeysLastWorkflows.RemoveRange(q);
            _DbContextProvider.Current.SaveChanges();
            _DbContextProvider.SaveAndCommit();
        }
    }
}