// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation
{
    public class KeysLastAuthorisationWriter : IKeysLastAuthorisationWriter
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly WorkflowDbContext _DbContextProvider;

        public KeysLastAuthorisationWriter(IUtcDateTimeProvider dateTimeProvider, WorkflowDbContext dbContextProvider)
        {
            _DateTimeProvider = dateTimeProvider;
            _DbContextProvider = dbContextProvider;
        }

        public async Task Execute(KeysLastAuthorisationArgs args)
        {
            var e = _DbContextProvider
                .KeyReleaseWorkflowStates
                .SingleOrDefault(x => x.LabConfirmationId == args.LabConfirmationID);

            if (e == null)
                return;

            e.Authorised = true;

            _DbContextProvider.KeyReleaseWorkflowStates.Update(e);
        }
    }
}