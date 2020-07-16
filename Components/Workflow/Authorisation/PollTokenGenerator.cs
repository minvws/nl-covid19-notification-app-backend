// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions;
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
            return _JwtService.GenerateCustomJwt(DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds(),
                new Dictionary<string, object>()
                {
                    ["payload"] = Guid.NewGuid().ToString() // make polltoken unique
                });
        }

        /// <summary>
        /// Get WorkflowState from DB by PollToken or LabConfirmationId identifier
        /// </summary>
        /// <param name="identifier">PollToken or LabConfirmationId identifier</param>
        /// <returns></returns>
        /// <exception cref="KeyReleaseWorkflowStateNotFoundException"></exception>
        private async Task<KeyReleaseWorkflowState> GetWfByIdentifier(string identifier)
        {
            var wf = await _DbContextProvider.KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .FirstOrDefaultAsync(state =>
                    state.PollToken == identifier || state.LabConfirmationId == identifier);

            if (wf != null)
            {
                return wf;
            }

            throw new KeyReleaseWorkflowStateNotFoundException();
        }

        private async Task<string> RefreshPollToken(KeyReleaseWorkflowState wf)
        {
            if (wf.Authorised && wf.LabConfirmationId != "")
                wf.LabConfirmationId = ""; // clear labconf.id when still full

            // generate new PollToken
            string newToken = Generate();

            wf.PollToken = newToken;
            await _DbContextProvider.KeyReleaseWorkflowStates.BatchUpdateAsync(wf);
            return newToken;
        }


        public async Task<string> ExecuteGenerationByLabConfirmationId(string identifier)
        {
            var wf = await GetWfByIdentifier(identifier);
            return await RefreshPollToken(wf);
        }

        public async Task<KeyReleaseWorkflowState> ExecuteGenerationByPollToken(string identifier)
        {
            _JwtService.IsValidJwt(identifier);
            var wf = await GetWfByIdentifier(identifier);
            await RefreshPollToken(wf);
            return wf;
        }
    }
}