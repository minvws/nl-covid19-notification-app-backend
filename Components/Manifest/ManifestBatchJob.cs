// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBatchJob
    {
        private readonly ManifestBuilderAndFormatter _BuilderAndFormatter;
        private readonly ContentDbContext _ContentDb;

        public ManifestBatchJob(ManifestBuilderAndFormatter builderAndFormatter, ContentDbContext contentDb)
        {
            _BuilderAndFormatter = builderAndFormatter ?? throw new ArgumentNullException(nameof(builderAndFormatter));
            _ContentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
        }

        public async Task Execute()
        {
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