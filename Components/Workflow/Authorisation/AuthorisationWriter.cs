// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Include(x => x.Keys)
                .SingleOrDefault(x => x.LabConfirmationId == args.LabConfirmationId);

            if (e == null)
                throw new LabConfirmationIdNotFoundException();

            e.AuthorisedByCaregiver = true;
            e.DateOfSymptomsOnset = args.DateOfSymptomsOnset;

            if (e.Keys != null && e.Keys.Any())
            {
                e.Authorised = true;
            }

            _DbContextProvider.KeyReleaseWorkflowStates.Update(e);
            return Task.CompletedTask;
        }
    }

    public class LabConfirmationIdNotFoundException : Exception
    {
    }
}