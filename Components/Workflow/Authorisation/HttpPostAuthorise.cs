// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostAuthorise
    {
        private readonly IAuthorisationWriter _AuthorisationWriter;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IReleaseKeysAuthorizationValidator _Validator;

        public HttpPostAuthorise(IAuthorisationWriter authorisationWriter, WorkflowDbContext dbContextProvider, IReleaseKeysAuthorizationValidator validator)
        {
            _AuthorisationWriter = authorisationWriter;
            _DbContextProvider = dbContextProvider;
            _Validator = validator;
        }

        public async Task<IActionResult> Execute(AuthorisationArgs args)
        {

            await _AuthorisationWriter.Execute(args);
            _DbContextProvider.SaveAndCommit();

            return new OkObjectResult(new AuthorisationResponse { Valid = true });
        }
    }
}