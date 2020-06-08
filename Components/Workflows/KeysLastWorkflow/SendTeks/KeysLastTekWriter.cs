// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class KeysLastTekWriter : IKeysLastTekWriter
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;

        public KeysLastTekWriter(DbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public void Execute(KeysLastReleaseTeksArgs args)
        {
            var e = _DbContextProvider.Current.KeysLastWorkflows
                .SingleOrDefault(x => x.TekWriteAuthorisationToken == args.Token && x.State == KeysLastWorkflowState.Receiving);

            if (e == null)
            {
                //TODO log miss
                return;
            }

            e.TekWriteAuthorisationToken = null;
            e.TekContent = JsonConvert.SerializeObject(args.Items);
            e.State = KeysLastWorkflowState.Authorised;

            _DbContextProvider.Current.KeysLastWorkflows.Update(e);
            _DbContextProvider.Current.SaveChangesAsync();

            //TODO log hit
        }
    }
}