// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DbProvision
{
    public class DbProvisionLoggingExtensions
    {
        private const string Name = "DbProvision";
        private const int Base = LoggingCodex.DbProvision;

        private const int Start = Base;
        private const int Finished = Base + 99;
        
        private const int WorkflowDb = Base + 1;
        private const int ContentDb = Base + 2;
        private const int JobDb = Base + 3;

        private readonly ILogger _Logger;

        public DbProvisionLoggingExtensions(ILogger<DbProvisionLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _Logger.LogInformation("[{name}/{id}] Start.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _Logger.LogInformation("[{name}/{id}] Complete.",
                Name, Finished);
        }

        public void WriteWorkFlowDb()
        {
            _Logger.LogInformation("[{name}/{id}] Workflow...",
                Name, WorkflowDb);
        }

        public void WriteContentDb()
        {
            _Logger.LogInformation("[{name}/{id}] Content...",
                Name, ContentDb);
        }

        public void WriteJobDb()
        {
            _Logger.LogInformation("[{name}/{id}] Job...",
                Name, JobDb);
        }
    }
}
