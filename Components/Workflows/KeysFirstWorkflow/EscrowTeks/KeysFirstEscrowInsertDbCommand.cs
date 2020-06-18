// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    public class KeysFirstEscrowInsertDbCommand : IKeysFirstEscrowWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;

        public KeysFirstEscrowInsertDbCommand(WorkflowDbContext contextProvider, IUtcDateTimeProvider utcDateTimeProvider)
        {
            _DbContextProvider = contextProvider;
            _UtcDateTimeProvider = utcDateTimeProvider;
        }

        public Task Execute(KeysFirstEscrowArgs args)
        {
            // var e = args.Keys.ToEntities();
            // await _DbContextProvider.AddRangeAsync(e);
            return Task.CompletedTask;
        }
    }
}
