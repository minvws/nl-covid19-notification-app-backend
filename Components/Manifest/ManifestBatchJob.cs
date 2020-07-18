// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{

    public class ManifestBatchJob
    {
        private readonly DynamicManifestReader _Reader;
        private readonly ContentDbContext _ContentDb;

        public ManifestBatchJob(DynamicManifestReader reader, ContentDbContext contentDb)
        {
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _ContentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
        }

        public async Task Execute()
        {
            try
            {
                _ContentDb.BeginTransaction();
                var e = await _Reader.Execute();
                var current = await new SafeBinaryContentDbReader<ManifestEntity>(_ContentDb).Execute(e.PublishingId);
                if (current == null)
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