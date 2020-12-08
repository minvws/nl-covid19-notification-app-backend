// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public class WorkflowDatabaseCreateCommand
    {
        private readonly WorkflowDbContext _Provider;

        public WorkflowDatabaseCreateCommand(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var config = new StandardEfDbConfig(configuration, "Workflow");
            var builder = new SqlServerDbContextOptionsBuilder(config, loggerFactory);
            _Provider = new WorkflowDbContext(builder.Build());
        }

        public async Task ExecuteAsync(bool nuke)
        {
            if (nuke) await _Provider.Database.EnsureDeletedAsync();
            await _Provider.Database.EnsureCreatedAsync();
        }
    }
}
