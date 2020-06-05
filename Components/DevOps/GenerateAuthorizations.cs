// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class GenerateAuthorizations
    {
        private readonly IDbContextProvider<WorkflowDbContext>_DbContextProvider;
        private readonly IWorkflowAuthorisationWriter _Writer;

        public GenerateAuthorizations(IDbContextProvider<WorkflowDbContext>dbContextProvider, IWorkflowAuthorisationWriter writer)
        {
            _DbContextProvider = dbContextProvider;
            _Writer = writer;
        }

        public async Task Execute(int pAuthorize, Random r)
        {
            _DbContextProvider.BeginTransaction();
            var unauthorized = _DbContextProvider.Current.Set<KeysFirstTekReleaseWorkflowEntity>()
                .Where(x => x.Authorised == false)
                .Select(x => x.AuthorisationToken)
                .ToArray();

            var authorized = unauthorized
                .Where(x => r.Next(100) <= pAuthorize);

            foreach (var i in authorized)
                await _Writer.Execute(new WorkflowAuthorisationArgs { Token = i});

            _DbContextProvider.SaveAndCommit();
        }
    }
}