// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateWorkflowDatabase
    {
        private readonly IDbContextProvider<WorkflowDbContext> _provider;

        public CreateWorkflowDatabase(IDbContextProvider<WorkflowDbContext> provider)
        {
            _provider = provider;
        }

        public async Task Execute()
        {
            await _provider.Current.Database.EnsureCreatedAsync();
        }

    }
}
