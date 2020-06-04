// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class TokenFirstSecretWriter : ITokenFirstSecretWriter
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public void Execute(string secretToken)
        {
            var e = new KeysLastTekReleaseWorkflowEntity
            {
                Created = _DateTimeProvider.Now(),
                SecretToken = secretToken,
                State = TokenFirstWorkflowState.Unauthorised
            };

            //TODO secret token already exists...
            _DbContextProvider.Current.KeysLastWorkflows.AddAsync(e);
            _DbContextProvider.Current.SaveChangesAsync();
        }
    }
}