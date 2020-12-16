// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace DbProvision
{
    public class DatabaseProvisioner
    {
        private readonly DbProvisionLoggingExtensions _Logger;

        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ContentDbContext _ContentDbContext;
        private readonly EksPublishingJobDbContext _EksPublishingJobDbContext;
        private readonly DataProtectionKeysDbContext _DataProtectionKeysDbContext;

        public DatabaseProvisioner(DbProvisionLoggingExtensions logger, WorkflowDbContext workflowDbContext, ContentDbContext contentDbContext, EksPublishingJobDbContext eksPublishingJobDbContext, DataProtectionKeysDbContext dataProtectionKeysDbContext)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _WorkflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _EksPublishingJobDbContext = eksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContext));
            _DataProtectionKeysDbContext = dataProtectionKeysDbContext ?? throw new ArgumentNullException(nameof(dataProtectionKeysDbContext));
        }

        public async Task ExecuteAsync(string[] args)
        {
            var nuke = !args.Contains("nonuke");

            _Logger.WriteStart();

            _Logger.WriteWorkFlowDb();
            await ProvisionWorkflow(nuke);

            _Logger.WriteContentDb();
            await ProvisionContent(nuke);

            _Logger.WriteJobDb();
            await ProvisionEksPublishingJob(nuke);

            _Logger.WriteDataProtectionKeysDb();
            await ProvisionDataProtectionKeys(nuke);

            _Logger.WriteFinished();
        }

        private async Task ProvisionWorkflow(bool nuke)
        {
            if (nuke)
            {
                await _WorkflowDbContext.Database.EnsureDeletedAsync();
            }
            await _WorkflowDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionContent(bool nuke)
        {
            if (nuke)
            {
                await _ContentDbContext.Database.EnsureDeletedAsync();
            }
            await _ContentDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionEksPublishingJob(bool nuke)
        {
            if (nuke)
            {
                await _EksPublishingJobDbContext.Database.EnsureDeletedAsync();
            }
            await _EksPublishingJobDbContext.Database.EnsureCreatedAsync();
        }

        private async Task ProvisionDataProtectionKeys(bool nuke)
        {
            if (nuke)
            {
                await _DataProtectionKeysDbContext.Database.EnsureDeletedAsync();
            }
            await _DataProtectionKeysDbContext.Database.EnsureCreatedAsync();
        }
    }
}
