// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeTekWriter : ITekWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public FakeTekWriter(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DbContextProvider = dbContextProvider;
            _UtcDateTimeProvider = utcDateTimeProvider;
        }

        /// <summary>
        /// May arrive BEFORE authorisation
        /// TODO limits on keys.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Execute(ReleaseTeksArgs args)
        {
            var wf = _DbContextProvider
                .KeyReleaseWorkflowStates
                .FirstOrDefault(x => x.BucketId == args.BucketId);

            if (wf == null)
                return;

            var entities = args.Keys.ToEntities();
            
            foreach (var e in entities)
                e.Owner = wf;

            await _DbContextProvider.TemporaryExposureKeys.AddRangeAsync(entities);
        }
    }
}