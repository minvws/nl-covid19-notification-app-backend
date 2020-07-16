// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostAuthorise
    {
        private readonly IAuthorisationWriter _AuthorisationWriter;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenGenerator _PollTokenGenerator;

        public HttpPostAuthorise(IAuthorisationWriter authorisationWriter, WorkflowDbContext dbContextProvider,
            PollTokenGenerator pollTokenGenerator)
        {
            _AuthorisationWriter = authorisationWriter;
            _DbContextProvider = dbContextProvider;
            _PollTokenGenerator = pollTokenGenerator;
        }

        public async Task<IActionResult> Execute(AuthorisationArgs args)
        {
            try
            {
                await _AuthorisationWriter.Execute(args);
                string pollToken = await _PollTokenGenerator.ExecuteGenerationByLabConfirmationId(args.LabConfirmationId);
                
                _DbContextProvider.SaveAndCommit();

                return new OkObjectResult(new AuthorisationResponse {Valid = true, PollToken = pollToken});
            }
            catch (KeyReleaseWorkflowStateNotFoundException e)
            {
                return new OkObjectResult(new AuthorisationResponse {Valid = false});
            }
        }
    }
}