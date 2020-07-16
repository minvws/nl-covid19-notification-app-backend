// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class LabVerifyChecker
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenGenerator _PollTokenGenerator;

        public LabVerifyChecker(WorkflowDbContext dbContextProvider, PollTokenGenerator pollTokenGenerator)
        {
            _DbContextProvider = dbContextProvider;
            _PollTokenGenerator = pollTokenGenerator;
        }

        public async Task<string> Execute(LabVerifyArgs args)
        {
            // check if polltoken is valid by querying on KeyReleaseWorkflowStates polltoken
            var wf = await _PollTokenGenerator.ExecuteGenerationByPollToken(args.PollToken);
            var keyCount = 0;
            if (wf != null)
            {
                keyCount = wf.Keys.Count;
            }
            _DbContextProvider.SaveAndCommit();

            if (wf != null && keyCount > 0)
            {
                return wf.PollToken; // positive
            }

            throw new LabVerifyKeysEmptyException(wf.PollToken);
        }
    }
}