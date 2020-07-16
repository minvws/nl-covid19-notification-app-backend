// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using JWT.Exceptions;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostLabVerify
    {
        private readonly LabVerifyChecker _LabVerifyChecker;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenGenerator _PollTokenGenerator;

        public HttpPostLabVerify(LabVerifyChecker labVerifyChecker, WorkflowDbContext dbContextProvider,
            PollTokenGenerator pollTokenGenerator)
        {
            _LabVerifyChecker = labVerifyChecker;
            _DbContextProvider = dbContextProvider;
            _PollTokenGenerator = pollTokenGenerator;
        }

        public async Task<IActionResult> Execute(LabVerifyArgs args)
        {
            try
            {
                await _LabVerifyChecker.Execute(args);
                // no exceptions so no empty bucket
                return new OkObjectResult(new AuthorisationResponse {Valid = true});
            }
            catch (LabVerifyKeysEmptyException exception)
            {
                return new OkObjectResult(new AuthorisationResponse
                    {Valid = false, PollToken = exception.FreshPollToken});
            }
            catch (TokenExpiredException e)
            {
                return new UnauthorizedObjectResult(new AuthorisationResponse {Valid = false});
            }
            catch (KeyReleaseWorkflowStateNotFoundException e)
            {
                return new NotFoundObjectResult(new AuthorisationResponse {Valid = false});
            }
        }
    }
}