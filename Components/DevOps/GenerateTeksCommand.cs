// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class GenerateTeksCommand
    {
        private Random _Random;
        private GenerateTeksCommandArgs _Args;

        private readonly WorkflowDbContext _WorkflowDb;

        public GenerateTeksCommand(WorkflowDbContext workflowDb)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
        }

        public void Execute(GenerateTeksCommandArgs args)
        {
            _WorkflowDb.EnsureNoChangesOrTransaction();
            _WorkflowDb.BeginTransaction();

            _Args = args;
            _Random = new Random(args.Seed);

            GenWorkflows();
            _WorkflowDb.SaveAndCommit();

        }

        private void GenWorkflows()
        {
            for (var i = 0; i < _Args.WorkflowCount; i++)
            {
                var workflow = new TekReleaseWorkflowStateEntity
                {
                    LabConfirmationId = _Args.Authorised ? null : "2L2587",
                    BucketId = new byte[32], //TODO from consts/settings
                    ConfirmationKey = new byte[32], //TODO from consts/settings
                    Created = DateTime.Now - TimeSpan.FromMinutes(_Random.Next(14 * 24 * 60 * 60)),
                    AuthorisedByCaregiver = DateTime.Now
                };
                _Random.NextBytes(workflow.BucketId);
                _Random.NextBytes(workflow.ConfirmationKey);

                _WorkflowDb.Set<TekReleaseWorkflowStateEntity>().Add(workflow);
                GenTeks(workflow);
            }

        }

        private void GenTeks(TekReleaseWorkflowStateEntity workflow)
        {
            var count = _Random.Next(1, _Args.TekCountPerWorkflow);

            for (var i = 0; i < count; i++)
            {
                var k = new TekEntity
                {
                    Owner = workflow,
                    PublishingState = PublishingState.Unpublished,
                    RollingPeriod = 1,
                    RollingStartNumber = 1,
                    KeyData = new byte[16],
                    Region = "NL"
                };
                _Random.NextBytes(k.KeyData);
                workflow.Keys.Add(k);
                _WorkflowDb.Set<TekEntity>().Add(k);
            }
        }
    }
}