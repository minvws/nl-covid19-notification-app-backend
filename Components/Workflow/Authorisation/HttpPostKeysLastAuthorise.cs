// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostKeysLastAuthorise
    {
        private readonly IKeysLastAuthorisationWriter _AuthorisationWriter;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IReleaseKeysAuthorizationValidator _Validator;

        public HttpPostKeysLastAuthorise(IKeysLastAuthorisationWriter authorisationWriter, WorkflowDbContext dbContextProvider, IReleaseKeysAuthorizationValidator validator)
        {
            _AuthorisationWriter = authorisationWriter;
            _DbContextProvider = dbContextProvider;
            _Validator = validator;
        }

        public async Task<IActionResult> Execute(KeysLastAuthorisationArgs args)
        {
            if (!_Validator.Valid(args.UploadAuthorisationToken)) //TODO check validation
                return new OkResult();

            await _AuthorisationWriter.Execute(args);
            _DbContextProvider.SaveAndCommit();
            return new OkResult();
        }
    }
}