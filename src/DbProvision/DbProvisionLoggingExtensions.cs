using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using System;
using Microsoft.Extensions.Logging;

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
        public void WriteDataProtectionKeysDb()
        {
            _Logger.LogInformation("[{name}/{id}] DataProtectionKeys...",
                Name, DataProtectionKeysDb);
        }


        public void WriteStatsDb()
        {
            _Logger.LogInformation("[{name}/{id}] Stats...",
                Name, StatsDb);
        }

        public void WriteDkSourceDb()
        {
            _Logger.LogInformation("[{name}/{id}] DkSource...",
                Name, DkSourceDb);
        }

        public void WriteIksInDb()
        {
            _Logger.LogInformation("[{name}/{id}] IksIn...",
                Name, IksInDb);
        }

        public void WriteIksOutDb()
        {
            _Logger.LogInformation("[{name}/{id}] IksOut...",
                Name, IksOutDb);
        }

        public void WriteIksPublishingJobDb()
        {
            _Logger.LogInformation("[{name}/{id}] eIksPublishingJob...",
                Name, IksPublishingJobDb);
        }
    }
}
