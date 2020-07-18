// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation.Exceptions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class AuthorisationWriter : IAuthorisationWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenGenerator _PollTokenGenerator;

        public AuthorisationWriter(WorkflowDbContext dbContextProvider, PollTokenGenerator pollTokenGenerator)
        {
            _DbContextProvider = dbContextProvider;
            _PollTokenGenerator = pollTokenGenerator;
        }

        public async Task<string> Execute(AuthorisationArgs args)
        {
            var wf = await  _DbContextProvider
                .KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
                throw new KeyReleaseWorkflowStateNotFoundException();

            wf.AuthorisedByCaregiver = true;
            wf.DateOfSymptomsOnset = args.DateOfSymptomsOnset;

            if (wf.Keys != null && wf.Keys.Any())
            {
                wf.Authorised = true;
            }

            wf.LabConfirmationId = "";
            
            // create polltoken
            string pollToken = _PollTokenGenerator.GenerateToken();
            wf.PollToken = pollToken;
            
            return pollToken;
        }
    }

 
}