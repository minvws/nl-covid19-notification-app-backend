// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands
{
    public class GenerateTeksCommand
    {
        private readonly IRandomNumberGenerator _Rng;
        private readonly Func<TekReleaseWorkflowStateCreate> _RegisterWriter;

        private GenerateTeksCommandArgs _Args;
        private Func<WorkflowDbContext> _WorkflowDb;

        public GenerateTeksCommand(IRandomNumberGenerator rng, Func<WorkflowDbContext> workflowDb, Func<TekReleaseWorkflowStateCreate> registerWriter)
        {
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _RegisterWriter = registerWriter ?? throw new ArgumentNullException(nameof(registerWriter));
        }

        public async Task ExecuteAsync(GenerateTeksCommandArgs args)
        {
            _Args = args;
            await GenWorkflowsAsync();
        }

        private async Task GenWorkflowsAsync()
        {
            for (var i = 0; i < _Args.WorkflowCount; i++)
            {
                var w = await _RegisterWriter().ExecuteAsync();
                GenTeks(w.Id);
            }
        }

        private void GenTeks(long workflowId)
        {
            using var dbc = _WorkflowDb();
            //Have to load referenced object into new context
            var owner = dbc.KeyReleaseWorkflowStates.Single(x => x.Id == workflowId);
            owner.AuthorisedByCaregiver = DateTime.UtcNow;
            owner.StartDateOfTekInclusion = DateTime.UtcNow.AddDays(-1);
            owner.IsSymptomatic = InfectiousPeriodType.Symptomatic;

            for (var i = 0; i < _Args.TekCountPerWorkflow; i++)
            {
                var k = new TekEntity
                {
                    Owner = owner,
                    PublishingState = PublishingState.Unpublished,
                    RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber(),
                    RollingPeriod = _Rng.Next(1, UniversalConstants.RollingPeriodRange.Hi),
                    KeyData = _Rng.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                    PublishAfter = DateTime.UtcNow,
                };
                owner.Teks.Add(k);
                dbc.TemporaryExposureKeys.Add(k);
            }
            dbc.SaveAndCommit();
        }
    }
}