// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class KeysLastSecretWriter : IKeysLastSecretWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public KeysLastSecretWriter(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
        }

        public async Task Execute(string secretToken)
        {
            var e = new KeysLastTeksWorkflowEntity
            {
                Created = _DateTimeProvider.Now(),
                SecretToken = secretToken,
                //TODO State = KeysLastWorkflowState.Unauthorised
            };

            //TODO secret token already exists...
            await _DbContextProvider.KeysLastWorkflows.AddAsync(e);
            _DbContextProvider.SaveAndCommit();
        }
    }
}