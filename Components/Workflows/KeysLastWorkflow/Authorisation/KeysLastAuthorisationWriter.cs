// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
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

        public Task Execute(KeysLastAuthorisationArgs args)
        {
            throw new NotImplementedException();

            //var e = _DbContextProvider
            //    .KeysLastWorkflows
            //    .SingleOrDefault(x => x.ExternalTestId == args.LabConfirmationID); //TODO probably the wrong mapping

            //if (e == null)
            //    //TODO log miss.
            //    return;

            //e.TekWriteAuthorisationToken = ;
            //e.AuthorisationWindowStart = _DateTimeProvider.Now();
            //e.State = KeysLastWorkflowState.Receiving;
            
            //_DbContextProvider.KeysLastWorkflows.Update(e);
            //await _DbContextProvider.SaveChangesAsync();
        }
    }
}