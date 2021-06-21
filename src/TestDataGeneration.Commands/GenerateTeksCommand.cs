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
        private readonly IRandomNumberGenerator _rng;
        private readonly Func<TekReleaseWorkflowStateCreateV2> _registerWriter;

        private GenerateTeksCommandArgs _args;
        private readonly Func<WorkflowDbContext> _workflowDb;

        public GenerateTeksCommand(IRandomNumberGenerator rng, Func<WorkflowDbContext> workflowDb, Func<TekReleaseWorkflowStateCreateV2> registerWriter)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _workflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _registerWriter = registerWriter ?? throw new ArgumentNullException(nameof(registerWriter));
        }

        public async Task ExecuteAsync(GenerateTeksCommandArgs args)
        {
            _args = args;
            await GenWorkflowsAsync();
        }

        private async Task GenWorkflowsAsync()
        {
            for (var i = 0; i < _args.WorkflowCount; i++)
            {
                var w = await _registerWriter().ExecuteAsync();
                GenTeks(w.Id);
            }
        }

        private void GenTeks(long workflowId)
        {
            using var dbc = _workflowDb();
            //Have to load referenced object into new context
            var owner = dbc.KeyReleaseWorkflowStates.Single(x => x.Id == workflowId);
            owner.AuthorisedByCaregiver = DateTime.UtcNow;
            owner.StartDateOfTekInclusion = DateTime.UtcNow.AddDays(-1);
            owner.IsSymptomatic = InfectiousPeriodType.Symptomatic;

            for (var i = 0; i < _args.TekCountPerWorkflow; i++)
            {
                var k = new TekEntity
                {
                    Owner = owner,
                    PublishingState = PublishingState.Unpublished,
                    RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber(),
                    RollingPeriod = _rng.Next(1, UniversalConstants.RollingPeriodRange.Hi),
                    KeyData = _rng.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                    PublishAfter = DateTime.UtcNow,
                };
                owner.Teks.Add(k);
                dbc.TemporaryExposureKeys.Add(k);
            }
            dbc.SaveAndCommit();
        }
    }
}
