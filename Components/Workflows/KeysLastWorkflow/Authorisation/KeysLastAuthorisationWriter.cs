// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation
{
    public class KeysLastAuthorisationWriter : IKeysLastAuthorisationWriter
    {

        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;

        public KeysLastAuthorisationWriter(IUtcDateTimeProvider dateTimeProvider, IDbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DateTimeProvider = dateTimeProvider;
            _DbContextProvider = dbContextProvider;
        }

        public void Execute(string token)
        {
            var e = new KeysLastTeksWorkflowEntity 
            { 
                Created = _DateTimeProvider.Now(),
                SecretToken = token,
                State = KeysLastWorkflowState.Unauthorised
            };
            
            _DbContextProvider.Current.KeysLastWorkflows.Add(e);
        }
    }
}