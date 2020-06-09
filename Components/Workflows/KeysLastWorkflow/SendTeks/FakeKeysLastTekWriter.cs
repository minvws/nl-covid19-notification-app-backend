// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class FakeKeysLastTekWriter : IKeysLastTekWriter
    {
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public FakeKeysLastTekWriter(IDbContextProvider<WorkflowDbContext> dbContextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DbContextProvider = dbContextProvider;
            _UtcDateTimeProvider = utcDateTimeProvider;
        }

        public void Execute(KeysLastReleaseTeksArgs args)
        {
            var e = new KeysLastTeksWorkflowEntity 
            {
                Created = _UtcDateTimeProvider.Now(),
                TekContent = JsonConvert.SerializeObject(args.Items),
                State = KeysLastWorkflowState.Authorised,
            };

            _DbContextProvider.Current.KeysLastWorkflows.Add(e);
        }
    }
}