// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class HttpPostAppConfigCommand
    {
        private readonly AppConfigInsertDbCommand _InsertDbCommand;
        private readonly AppConfigValidator _Validator;
        private readonly ContentDbContext _Context;
        private readonly ILogger _Logger;

        public HttpPostAppConfigCommand(AppConfigInsertDbCommand insertDbCommand, AppConfigValidator validator, ContentDbContext context, ILogger<HttpPostAppConfigCommand> logger)
        {
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _Context = context ?? throw new ArgumentNullException(nameof(context));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute(AppConfigArgs args)
        {
            if (!_Validator.Valid(args))
            {
                _Logger.LogWarning("Bad request.");
                return new BadRequestResult();
            }

            _Logger.LogDebug("Writing DB.");
            await _InsertDbCommand.Execute(args);
            _Logger.LogDebug("Committing.");
            _Context.SaveAndCommit();
            _Logger.LogInformation($"Committed.");
            return new OkResult();
        }
    }
}
