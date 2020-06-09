// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.Authorisation
{
    public class KeysFirstDbAuthoriseCommand : IKeysFirstAuthorisationWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public KeysFirstDbAuthoriseCommand(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public async Task Execute(KeysFirstAuthorisationArgs args)
        {
            var workflow = _DbContextProvider.Set<KeysFirstTeksWorkflowEntity>()
                .Where(x => x.AuthorisationToken == args.Token)
                .Take(1)
                .ToArray()
                .SingleOrDefault();

            if (workflow == null)
                //TODO log a miss.
                return;

            workflow.Authorised = true;
            _DbContextProvider.Update(workflow);
            await _DbContextProvider.SaveChangesAsync();
        }
    }
}
