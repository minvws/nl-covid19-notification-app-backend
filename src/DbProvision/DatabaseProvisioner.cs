// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

namespace DbProvision
{
    public class DatabaseProvisioner
    {
        private readonly DbProvisionLoggingExtensions _logger;

        private readonly WorkflowDbContext _workflowDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobDbContext;
        private readonly DataProtectionKeysDbContext _dataProtectionKeysDbContext;
        private readonly StatsDbContext _statsDbContext;
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly IksInDbContext _iksInDbContext;
        private readonly IksOutDbContext _iksOutDbContext;
        private readonly IksPublishingJobDbContext _iksPublishingJobDbContext;

        public DatabaseProvisioner(DbProvisionLoggingExtensions logger,
            WorkflowDbContext workflowDbContext,
            ContentDbContext contentDbContext,
            EksPublishingJobDbContext eksPublishingJobDbContext,
            DataProtectionKeysDbContext dataProtectionKeysDbContext,
            StatsDbContext statsDbContext,
            DkSourceDbContext dkSourceDbContext,
            IksInDbContext iksInDbContext,
            IksOutDbContext iksOutDbContext,
            IksPublishingJobDbContext iksPublishingJobDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _eksPublishingJobDbContext = eksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContext));
            _dataProtectionKeysDbContext = dataProtectionKeysDbContext ?? throw new ArgumentNullException(nameof(dataProtectionKeysDbContext));
            _statsDbContext = statsDbContext ?? throw new ArgumentNullException(nameof(statsDbContext));
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
            _iksInDbContext = iksInDbContext ?? throw new ArgumentNullException(nameof(iksInDbContext));
            _iksOutDbContext = iksOutDbContext ?? throw new ArgumentNullException(nameof(iksOutDbContext));
            _iksPublishingJobDbContext = iksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContext));

        }

        public async Task ExecuteAsync(string[] args)
        {
            var nuke = !args.Contains("nonuke");

            _logger.WriteStart();

            _logger.WriteWorkFlowDb();
            await ProvisionWorkflow(nuke);

            _logger.WriteContentDb();
            await ProvisionContent(nuke);

            _logger.WriteJobDb();
            await ProvisionEksPublishingJob(nuke);

            _logger.WriteDataProtectionKeysDb();
            await ProvisionDataProtectionKeys(nuke);

            _logger.WriteStatsDb();
            await ProvisionStats(nuke);

            _logger.WriteDkSourceDb();
            await ProvisionDkSource(nuke);

            _logger.WriteIksInDb();
            await ProvisionIksIn(nuke);

            _logger.WriteIksOutDb();
            await ProvisionIksOut(nuke);

            _logger.WriteIksPublishingJobDb();
            await ProvisionIksPublishingJob(nuke);

            _logger.WriteFinished();
        }

        private async Task ProvisionWorkflow(bool nuke)
        {
            if (nuke)
            {
                await _workflowDbContext.Database.EnsureDeletedAsync();
            }
            await _workflowDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionContent(bool nuke)
        {
            if (nuke)
            {
                await _contentDbContext.Database.EnsureDeletedAsync();
            }
            await _contentDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionEksPublishingJob(bool nuke)
        {
            if (nuke)
            {
                await _eksPublishingJobDbContext.Database.EnsureDeletedAsync();
            }
            await _eksPublishingJobDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionDataProtectionKeys(bool nuke)
        {
            if (nuke)
            {
                await _dataProtectionKeysDbContext.Database.EnsureDeletedAsync();
            }
            await _dataProtectionKeysDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionStats(bool nuke)
        {
            if (nuke)
            {
                await _statsDbContext.Database.EnsureDeletedAsync();
            }
            await _statsDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionDkSource(bool nuke)
        {
            if (nuke)
            {
                await _dkSourceDbContext.Database.EnsureDeletedAsync();
            }
            await _dkSourceDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionIksIn(bool nuke)
        {
            if (nuke)
            {
                await _iksInDbContext.Database.EnsureDeletedAsync();
            }
            await _iksInDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionIksOut(bool nuke)
        {
            if (nuke)
            {
                await _iksOutDbContext.Database.EnsureDeletedAsync();
            }
            await _iksOutDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionIksPublishingJob(bool nuke)
        {
            if (nuke)
            {
                await _iksPublishingJobDbContext.Database.EnsureDeletedAsync();
            }
            await _iksPublishingJobDbContext.Database.EnsureCreatedAsync();
        }
    }
}
