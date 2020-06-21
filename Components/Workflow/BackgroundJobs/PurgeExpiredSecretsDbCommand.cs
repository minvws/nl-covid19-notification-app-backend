// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.BackgroundJobs
{
    public class PurgeExpiredSecretsDbCommand
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public PurgeExpiredSecretsDbCommand(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute()
        {
            var entityType = _DbContextProvider.Model.FindEntityType(typeof(KeyReleaseWorkflowState));
            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();
            var query = $"DELETE FROM [{schema}].[{tableName}] WHERE {nameof(KeyReleaseWorkflowState.ValidUntil)} <= GETDATE()";
            await _DbContextProvider.Database.ExecuteSqlRawAsync(query);
            _DbContextProvider.SaveAndCommit();

            return new OkObjectResult(true);
        }
    }
}