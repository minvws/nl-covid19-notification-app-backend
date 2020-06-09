// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class KeysLastSecretWriter : IKeysLastSecretWriter
    {
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public KeysLastSecretWriter(IDbContextProvider<WorkflowDbContext> dbContextProvider, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
        }

        public void Execute(string secretToken)
        {
            var e = new KeysLastTeksWorkflowEntity
            {
                Created = _DateTimeProvider.Now(),
                SecretToken = secretToken,
                State = KeysLastWorkflowState.Unauthorised
            };

            //TODO secret token already exists...
            _DbContextProvider.Current.KeysLastWorkflows.AddAsync(e);
            _DbContextProvider.SaveAndCommit();
        }
    }
}