// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class GenerateTeksCommand
    {
        private readonly IRandomNumberGenerator _Rng;
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly Func<TekReleaseWorkflowStateCreate> _RegisterWriter;

        private GenerateTeksCommandArgs _Args;

        public GenerateTeksCommand(IRandomNumberGenerator rng, WorkflowDbContext workflowDb, Func<TekReleaseWorkflowStateCreate> registerWriter)
        {
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _RegisterWriter = registerWriter ?? throw new ArgumentNullException(nameof(registerWriter));
        }

        public async Task Execute(GenerateTeksCommandArgs args)
        {
            _WorkflowDb.EnsureNoChangesOrTransaction();

            _Args = args;
            await GenWorkflows();
        }

        private async Task GenWorkflows()
        {
            for (var i = 0; i < _Args.WorkflowCount; i++)
            {
                _WorkflowDb.BeginTransaction();
                var workflow = await _RegisterWriter().Execute(); //Save/Commit

                _WorkflowDb.BeginTransaction();
                GenTeks(workflow);
                _WorkflowDb.SaveAndCommit();
            }
        }

        private void GenTeks(TekReleaseWorkflowStateEntity workflow)
        {
            var count = _Rng.Next(1, _Args.TekCountPerWorkflow);

            for (var i = 0; i < count; i++)
            {
                var k = new TekEntity
                {
                    Owner = workflow,
                    PublishingState = PublishingState.Unpublished,
                    RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber(),
                    RollingPeriod = _Rng.Next(1, 144),
                    KeyData = _Rng.NextByteArray(16),
                    PublishAfter = DateTime.UtcNow,
                    Region = "NL"
                };
                workflow.Teks.Add(k);
                _WorkflowDb.Set<TekEntity>().Add(k);
            }
        }
    }
}