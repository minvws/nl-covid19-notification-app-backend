// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostAuthorise
    {
        private readonly IAuthorisationWriter _AuthorisationWriter;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly ILogger _Logger;

        public HttpPostAuthorise(IAuthorisationWriter authorisationWriter, WorkflowDbContext dbContextProvider, ILogger logger)
        {
            _AuthorisationWriter = authorisationWriter ?? throw new ArgumentNullException(nameof(authorisationWriter));
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute(AuthorisationArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            try
            {
                _Logger.Debug($"Authorisation - {args.LabConfirmationId}.");
                await _AuthorisationWriter.Execute(args);
                _DbContextProvider.SaveAndCommit();
                _Logger.Debug($"Authorisation complete - Valid:true.");
                return new OkObjectResult(new AuthorisationResponse {Valid = true});
            }
            catch (Exception e) //TODO general catch -> specific or just branch on invalid/unauthorised.
            {
                _Logger.Error(e.ToString());
                return new OkObjectResult(new AuthorisationResponse {Valid = false});
            }
        }
    }
}