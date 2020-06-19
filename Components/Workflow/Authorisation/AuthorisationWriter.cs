// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class AuthorisationWriter : IAuthorisationWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public AuthorisationWriter(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public Task Execute(AuthorisationArgs args)
        {
            var e = _DbContextProvider
                .KeyReleaseWorkflowStates
                .SingleOrDefault(x => x.LabConfirmationId == args.LabConfirmationId);

            if (e == null)
                return Task.CompletedTask;

            e.AuthorisedByCaregiver = true;

            if (e.Keys != null && e.Keys.Any())
                e.Authorised = true;

            _DbContextProvider.KeyReleaseWorkflowStates.Update(e);
            return Task.CompletedTask;
        }
    }
}