// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBatchJob
    {
        private readonly ManifestBuilderAndFormatter _BuilderAndFormatter;
        private readonly ContentDbContext _ContentDb;
        private readonly ILogger _Logger;

        public ManifestBatchJob(ManifestBuilderAndFormatter builderAndFormatter, ContentDbContext contentDb, ILogger<ManifestBatchJob> logger)
        {
            _BuilderAndFormatter = builderAndFormatter ?? throw new ArgumentNullException(nameof(builderAndFormatter));
            _ContentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute()
        {
            if (!WindowsIdentityStuff.CurrentUserIsAdministrator())
                _Logger.LogWarning("{ManifestBatchJobName} started WITHOUT elevated privileges - errors may occur when signing content.", nameof(ManifestBatchJob));

            try
            {
                _ContentDb.BeginTransaction();
                var e = await _BuilderAndFormatter.Execute();
                if (e == null)
                    return;

                _ContentDb.Add(e);
                _ContentDb.SaveAndCommit();
            }
            finally
            {
                await _ContentDb.DisposeAsync();
            }

        }
    }
}