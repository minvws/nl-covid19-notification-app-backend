// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ContentLoading
{
    public class HttpPostGenericContentCommand
    {
        private readonly GenericContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly ContentDbContext _DbContext;
        private readonly ILogger _Logger;

        public HttpPostGenericContentCommand(GenericContentValidator validator, ContentInsertDbCommand insertDbCommand, ContentDbContext dbContext, ILogger logger)
        {
            _Validator = validator;
            _InsertDbCommand = insertDbCommand;
            _DbContext = dbContext;
            _Logger = logger;
        }

        public async Task<IActionResult> Execute(GenericContentArgs args)
        {
            if (!_Validator.IsValid(args))
            {
                _Logger.LogWarning("Bad request.");
                return new BadRequestResult();
            }

            _Logger.LogDebug("Writing DB.");
            await _InsertDbCommand.Execute(args);
            _Logger.LogDebug("Committing.");
            _DbContext.SaveAndCommit();
            _Logger.LogInformation($"Committed.");
            return new OkResult();
        }
    }
}
