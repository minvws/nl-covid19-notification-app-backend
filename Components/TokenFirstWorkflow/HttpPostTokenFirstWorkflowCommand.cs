// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class HttpPostTokenFirstWorkflowCommand
    {
        private readonly ITokenFirstWorkflowValidator _Validator;
        private readonly ITokenFirstWorkflowWriter _Writer;
        private readonly IDbContextProvider<ExposureContentDbContext> _DbContextProvider;

        public HttpPostTokenFirstWorkflowCommand(ITokenFirstWorkflowValidator validator, ITokenFirstWorkflowWriter writer, DbContextProvider<ExposureContentDbContext> dbContextProvider)
        {
            _Validator = validator;
            _Writer = writer;
            _DbContextProvider = dbContextProvider;
        }

        public IActionResult Execute(WorkflowArgs args)
        {
            if (!_Validator.Validate(args))
            {
                //TODO log bad request
                return new OkResult();
            }

            if (!_Validator.Validate(args))
                return new BadRequestResult();

            _DbContextProvider.BeginTransaction();
            _Writer.Execute(args);
            _DbContextProvider.SaveAndCommit();
            return new OkResult();
        }
    }
}