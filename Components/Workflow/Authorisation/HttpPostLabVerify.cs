// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

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

        public HttpPostLabVerify(LabVerifyChecker labVerifyChecker, WorkflowDbContext dbContextProvider)
        {
            _LabVerifyChecker = labVerifyChecker;
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute(LabVerifyArgs args)
        {
            try
            {
                var response = await _LabVerifyChecker.Execute(args);
                
                _DbContextProvider.SaveAndCommit();
                
                return new OkObjectResult(response);
            }
            catch (InvalidTokenPartsException)
            {
                return new UnauthorizedObjectResult(new AuthorisationResponse {Valid = false});
            }
            catch (TokenExpiredException)
            {
                return new UnauthorizedObjectResult(new AuthorisationResponse {Valid = false});
            }
            catch (KeyReleaseWorkflowStateNotFoundException)
            {
                return new NotFoundObjectResult(new AuthorisationResponse {Valid = false});
            }
        }
    }
}