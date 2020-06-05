// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class HttpPostWorkflowCommand
    {
        private readonly IWorkflowWriter _WorkflowDbInsertCommand;
        private readonly IWorkflowValidator _WorkflowValidator;
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;

        public HttpPostWorkflowCommand(IWorkflowWriter infectionDbInsertCommand, IWorkflowValidator WorkflowValidator, IDbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _WorkflowDbInsertCommand = infectionDbInsertCommand;
            _WorkflowValidator = WorkflowValidator;
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute(WorkflowArgs args)
        {
            if (!_WorkflowValidator.Validate(args))
                return new BadRequestResult();

            await using (var tx = _DbContextProvider.BeginTransaction())
            {
                await _WorkflowDbInsertCommand.Execute(args);
                await _DbContextProvider.Current.SaveChangesAsync();
                await tx.CommitAsync();
            }

            return new OkResult();
        }
    }
}