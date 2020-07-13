// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class PollTokenGenerator
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly JwtService _JwtService;

        public PollTokenGenerator(WorkflowDbContext dbContextProvider, JwtService jwtService)
        {
            _DbContextProvider = dbContextProvider;
            _JwtService = jwtService;
        }

        private string Generate()
        {
            return _JwtService.GenerateCustomJwt(DateTimeOffset.UtcNow.AddSeconds(20).ToUnixTimeSeconds());
        }

        private async Task<KeyReleaseWorkflowState?> ProcessId(string identifier)
        {
            var e = await _DbContextProvider.KeyReleaseWorkflowStates.Include(x => x.Keys).SingleOrDefaultAsync(state =>
                state.PollToken == identifier || state.LabConfirmationId == identifier);
            if (e != null)
            {
                if (e.Authorised && e.LabConfirmationId != "")
                    e.LabConfirmationId = ""; // clear labconf.id when still full


                // generate new PollToken
                e.PollToken = Generate();
                _DbContextProvider.KeyReleaseWorkflowStates.Update(e);
            }

            return e;
        }


        public async Task<KeyReleaseWorkflowState?> ExecuteGenerationByLabConfirmationId(string identifier)
        {
            //TODO Add validation on identifier
            return await ProcessId(identifier);
        }

        public async Task<KeyReleaseWorkflowState?> ExecuteGenerationByPollToken(string identifier)
        {
            //TODO Add validation on identifier
            _JwtService.IsValidJwt(identifier);
            return await ProcessId(identifier);
        }
    }
}