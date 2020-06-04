// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components
{
    public class GenerateAgAuthorisations
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;
        private readonly IWorkflowAuthorisationWriter _Writer;

        public GenerateAgAuthorisations(IDbContextProvider<ExposureContentDbContext>dbContextProvider, IWorkflowAuthorisationWriter writer)
        {
            _DbContextProvider = dbContextProvider;
            _Writer = writer;
        }

        public void Execute(int pAuthorise, Random r)
        {
            var unauthorised = _DbContextProvider.Current.Set<KeysFirstTekReleaseWorkflowEntity>()
                .Where(x => x.Authorised == false)
                .Select(x => x.AuthorisationToken)
                .ToArray();

            var authorised = unauthorised
                .Where(x => r.Next(100) <= pAuthorise);

            foreach (var i in authorised)
                _Writer.Execute(new WorkflowAuthorisationArgs { Token = i}).GetAwaiter().GetResult();
        }
    }
}