// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class FakeKeysLastTekWriter : IKeysLastTekWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public FakeKeysLastTekWriter(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DbContextProvider = dbContextProvider;
            _UtcDateTimeProvider = utcDateTimeProvider;
        }

        public async Task Execute(KeysLastReleaseTeksArgs args)
        {
            var e = new KeyReleaseWorkflowState 
            {
                Created = _UtcDateTimeProvider.Now(),
                //TODO add keys to new table //TekContent = JsonConvert.SerializeObject(args.Items),
                //TODO only authorise after both keys and auth have been received //State = KeysLastWorkflowState.Authorised,
            };

            await _DbContextProvider.KeyReleaseWorkflowStates.AddAsync(e);
        }
    }
}