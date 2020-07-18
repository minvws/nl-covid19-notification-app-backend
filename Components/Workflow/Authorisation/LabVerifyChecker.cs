// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using JWT.Exceptions;
using Microsoft.EntityFrameworkCore;
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

        public async Task<LabVerifyResponse> Execute(LabVerifyArgs args)
        {
            if (!_PollTokenGenerator.Verify(args.PollToken))
            {
                throw new InvalidTokenPartsException("payload");
            }

            var wf = await _DbContextProvider.KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .FirstOrDefaultAsync(state =>
                    state.PollToken == args.PollToken);
            
            if (wf == null)
                throw new KeyReleaseWorkflowStateNotFoundException();

            string refreshedToken = _PollTokenGenerator.GenerateToken();
            wf.PollToken = refreshedToken;

            return new LabVerifyResponse()
                {PollToken = refreshedToken, Valid = wf.Keys != null && wf.Keys.Any()};
        }
    }
}