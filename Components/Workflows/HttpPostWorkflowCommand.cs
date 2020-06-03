// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class HttpPostWorkflowCommand
    {
        private readonly IWorkflowWriter _WorkflowDbInsertCommand;
        private readonly IWorkflowValidator _WorkflowValidator;
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;

        public HttpPostWorkflowCommand(IWorkflowWriter infectionDbInsertCommand, IWorkflowValidator WorkflowValidator, IDbContextProvider<ExposureContentDbContext>dbContextProvider)
        {
            _WorkflowDbInsertCommand = infectionDbInsertCommand;
            _WorkflowValidator = WorkflowValidator;
            _DbContextProvider = dbContextProvider;
        }

        public IActionResult Execute(WorkflowArgs args)
        {
            if (!_WorkflowValidator.Validate(args))
                return new BadRequestResult();

            using (var tx = _DbContextProvider.BeginTransaction())
            {
                _WorkflowDbInsertCommand.Execute(args).GetAwaiter().GetResult();
                _DbContextProvider.Current.SaveChanges();
                tx.Commit();
            }

            return new OkResult();
        }
    }
}