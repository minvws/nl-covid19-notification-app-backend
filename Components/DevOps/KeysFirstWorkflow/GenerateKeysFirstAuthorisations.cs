// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class GenerateKeysFirstAuthorisations
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IKeysFirstAuthorisationWriter _Writer;

        public GenerateKeysFirstAuthorisations(WorkflowDbContext dbContextProvider, IKeysFirstAuthorisationWriter writer)
        {
            _DbContextProvider = dbContextProvider;
            _Writer = writer;
        }

        public async Task Execute(int pAuthorize, Random r)
        {
            _DbContextProvider.BeginTransaction();
            var unauthorized = _DbContextProvider.Set<KeyReleaseWorkflowState>()
                .Where(x => x.Authorised == false)
                .Select(x => x.ConfirmationKey)
                .ToArray();

            var authorized = unauthorized
                .Where(x => r.Next(100) <= pAuthorize);

            foreach (var i in authorized)
                await _Writer.Execute(new KeysFirstAuthorisationArgs { Token = i});

            _DbContextProvider.SaveAndCommit();
        }
    }
}