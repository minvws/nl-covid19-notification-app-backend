// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class WorkflowInsertDbCommand : IWorkflowWriter
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public WorkflowInsertDbCommand(IDbContextProvider<ExposureContentDbContext>contextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DbContextProvider = contextProvider;
            _UtcDateTimeProvider = utcDateTimeProvider;
        }

        public async Task Execute(WorkflowArgs args)
        {
            var e = args.ToEntity();
            e.Created = _UtcDateTimeProvider.Now();
            await _DbContextProvider.Current.AddAsync(e);
        }
    }
}
