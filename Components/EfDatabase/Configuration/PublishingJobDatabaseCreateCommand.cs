// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public class PublishingJobDatabaseCreateCommand
    {
        private readonly EksPublishingJobDbContext _DbContextProvider;

        public PublishingJobDatabaseCreateCommand(EksPublishingJobDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
        }

        public async Task ExecuteAsync(bool nuke)
        {
            if (nuke) await _DbContextProvider.Database.EnsureDeletedAsync();
            await _DbContextProvider.Database.EnsureCreatedAsync();
        }
    }
}