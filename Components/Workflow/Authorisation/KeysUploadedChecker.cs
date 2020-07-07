// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class KeysUploadedChecker
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public KeysUploadedChecker(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public Task Execute(AuthorisationArgs args)
        {
            var e = _DbContextProvider
                .KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .SingleOrDefault(x => x.LabConfirmationId == args.LabConfirmationId && x.DateOfSymptomsOnset == args.DateOfSymptomsOnset);
            return e != null && e.Keys.Count > 0 ? Task.CompletedTask : throw new KeysUploadedNotValidException(e.Keys.Count.ToString());
        }
    }

    public class KeysUploadedNotValidException : Exception
    {
        public KeysUploadedNotValidException():base(){}
        public KeysUploadedNotValidException(string message):base(message){}
    }
}