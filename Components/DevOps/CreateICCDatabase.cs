// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateIccDatabase
    {
        private readonly IccBackendContentDbContext _Provider;

        public CreateIccDatabase(IConfiguration configuration)
        {
            var config = new StandardEfDbConfig(configuration, "Icc");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _Provider = new IccBackendContentDbContext(builder.Build());
        }

        public async Task Execute()
        {
            await _Provider.Database.EnsureDeletedAsync();
            await _Provider.Database.EnsureCreatedAsync();
        }

    }
}
