// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateWorkflowDatabase
    {
        private readonly IDbContextProvider<WorkflowDbContext> _Provider;

        public CreateWorkflowDatabase(IConfiguration configuration)
        {
            var config = new StandardEfDbConfig(configuration, "Workflow");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _Provider = new DbContextProvider<WorkflowDbContext>(new WorkflowDbContext(builder.Build()));
        }

        public async Task Execute()
        {
            await _Provider.Current.Database.EnsureCreatedAsync();
        }

    }
}
