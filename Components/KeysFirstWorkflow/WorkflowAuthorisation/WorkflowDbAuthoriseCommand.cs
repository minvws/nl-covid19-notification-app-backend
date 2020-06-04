// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation
{
    public class WorkflowDbAuthoriseCommand : IWorkflowAuthorisationWriter
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;

        public WorkflowDbAuthoriseCommand(IDbContextProvider<ExposureContentDbContext>dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public async Task Execute(WorkflowAuthorisationArgs args)
        {
            var Workflow = _DbContextProvider.Current.Set<KeysFirstTekReleaseWorkflowEntity>()
                .Where(x => x.AuthorisationToken == args.Token)
                .Take(1)
                .ToArray()
                .SingleOrDefault();

            if (Workflow == null)
                //TODO log a miss.
                return;

            Workflow.Authorised = true;
            _DbContextProvider.Current.Update(Workflow);
        }
    }
}
