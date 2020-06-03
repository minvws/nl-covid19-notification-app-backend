// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class TokenFirstAuthorisationWriter : ITokenFirstAuthorisationWriter
    {

        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;

        public TokenFirstAuthorisationWriter(IUtcDateTimeProvider dateTimeProvider, IDbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DateTimeProvider = dateTimeProvider;
            _DbContextProvider = dbContextProvider;
        }

        public void Execute(string token)
        {
            var e = new TokenFirstWorkflowEntity 
            { 
                Created = _DateTimeProvider.Now(),
                SecretToken = token,
                State = TokenFirstWorkflowState.Unauthorised
            };
            
            _DbContextProvider.Current.TokenFirstWorkflows.Add(e);
        }
    }
}