// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace DbProvision
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
        private const int DataProtectionKeysDb = Base + 4;
        private const int StatsDb = Base + 5;
        private const int DkSourceDb = Base + 6;
        private const int IksInDb = Base + 7;
        private const int IksOutDb = Base + 8;
        private const int IksPublishingJobDb = Base + 9;

        private readonly ILogger _logger;

        public DbProvisionLoggingExtensions(ILogger<DbProvisionLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _logger.LogInformation("[{name}/{id}] Start.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _logger.LogInformation("[{name}/{id}] Complete.",
                Name, Finished);
        }

        public void WriteWorkFlowDb()
        {
            _logger.LogInformation("[{name}/{id}] Workflow...",
                Name, WorkflowDb);
        }

        public void WriteContentDb()
        {
            _logger.LogInformation("[{name}/{id}] Content...",
                Name, ContentDb);
        }

        public void WriteJobDb()
        {
            _logger.LogInformation("[{name}/{id}] Job...",
                Name, JobDb);
        }
        public void WriteDataProtectionKeysDb()
        {
            _logger.LogInformation("[{name}/{id}] DataProtectionKeys...",
                Name, DataProtectionKeysDb);
        }


        public void WriteStatsDb()
        {
            _logger.LogInformation("[{name}/{id}] Stats...",
                Name, StatsDb);
        }

        public void WriteDkSourceDb()
        {
            _logger.LogInformation("[{name}/{id}] DkSource...",
                Name, DkSourceDb);
        }

        public void WriteIksInDb()
        {
            _logger.LogInformation("[{name}/{id}] IksIn...",
                Name, IksInDb);
        }

        public void WriteIksOutDb()
        {
            _logger.LogInformation("[{name}/{id}] IksOut...",
                Name, IksOutDb);
        }

        public void WriteIksPublishingJobDb()
        {
            _logger.LogInformation("[{name}/{id}] eIksPublishingJob...",
                Name, IksPublishingJobDb);
        }
    }
}
