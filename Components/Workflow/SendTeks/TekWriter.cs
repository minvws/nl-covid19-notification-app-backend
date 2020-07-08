// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class TekWriter : ITekWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly ILogger<WorkflowDbContext> _Logger;

        public TekWriter(WorkflowDbContext dbContextProvider, ILogger<WorkflowDbContext> logger)
        {
            _DbContextProvider = dbContextProvider;
            _Logger = logger;
        }

        public async Task Execute(ReleaseTeksArgs args)
        {
            var wf = _DbContextProvider
                .KeyReleaseWorkflowStates
                .FirstOrDefault(x => x.BucketId == args.BucketID);

            if (wf == null)
            {
                _Logger.LogInformation($"Workflow with bucketId {args.BucketID} not found.");
                return;
            }

            var entities = args.Keys.ToEntities();
            
            foreach (var e in entities)
                e.Owner = wf;

            if (wf.AuthorisedByCaregiver)
                wf.Authorised = true;

            await _DbContextProvider.TemporaryExposureKeys.AddRangeAsync(entities);
        }
    }
}