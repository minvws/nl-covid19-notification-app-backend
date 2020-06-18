// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.BackgroundJobs
{
    
    /// <summary>
    /// TODO check conditions for expiry
    /// </summary>
    public class SecretExpireDbCommand
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IWorkflowConfig _WorkflowConfig;

        public SecretExpireDbCommand(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IWorkflowConfig tokenFirstWorkflowConfig)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
            _WorkflowConfig = tokenFirstWorkflowConfig;
        }

        public void Execute()
        {
            var expired = _DateTimeProvider.Now() - TimeSpan.FromMinutes(_WorkflowConfig.AuthorisationWindowDurationMinutes);

            _DbContextProvider.BeginTransaction();

            throw new NotImplementedException();

            //var q = _DbContextProvider.KeysLastWorkflows
            //    .Where(x => x.State == KeysLastWorkflowState.Receiving && x.AuthorisationWindowStart < expired);

            //_DbContextProvider.KeysLastWorkflows.RemoveRange(q);
            _DbContextProvider.SaveChanges();
            _DbContextProvider.SaveAndCommit();
        }
    }
}