// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class TokenFirstWorkflowWriter : ITokenFirstWorkflowWriter
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;

        public TokenFirstWorkflowWriter(DbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public void Execute(WorkflowArgs args)
        {
            var e = _DbContextProvider.Current.KeysLastWorkflows
                .SingleOrDefault(x => x.TekWriteAuthorisationToken == args.Token && x.State == TokenFirstWorkflowState.Receiving);

            if (e == null)
            {
                //TODO log miss
                return;
            }

            e.TekWriteAuthorisationToken = null;
            e.TekContent = JsonConvert.SerializeObject(args.Items);
            e.State = TokenFirstWorkflowState.Authorised;

            _DbContextProvider.Current.KeysLastWorkflows.Update(e);
            _DbContextProvider.Current.SaveChangesAsync();

            //TODO log hit
        }
    }
}