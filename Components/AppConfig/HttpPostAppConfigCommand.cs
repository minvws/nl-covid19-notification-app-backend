// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class HttpPostAppConfigCommand
    {
        private readonly AppConfigInsertDbCommand _InsertDbCommand;
        private readonly AppConfigValidator _Validator;
        private readonly ExposureContentDbContext _Context;

        public HttpPostAppConfigCommand(AppConfigInsertDbCommand insertDbCommand, AppConfigValidator validator, ExposureContentDbContext context)
        {
            _InsertDbCommand = insertDbCommand;
            _Validator = validator;
            _Context = context;
        }

        public async Task<IActionResult> Execute(AppConfigArgs args)
        {
            if (!_Validator.Valid(args))
                return new BadRequestResult();

            await _InsertDbCommand.Execute(args);
            _Context.SaveAndCommit();
            return new OkResult();
        }
    }
}
