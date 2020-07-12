// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class LabVerifyChecker
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenGenerator _PollTokenGenerator;

        public LabVerifyChecker(WorkflowDbContext dbContextProvider, PollTokenGenerator pollTokenGenerator)
        {
            _DbContextProvider = dbContextProvider;
            _PollTokenGenerator = pollTokenGenerator;
        }

        public async Task<KeyReleaseWorkflowState?> Execute(LabVerifyArgs args)
        {
            // check if polltoken is valid by querying on KeyReleaseWorkflowStates polltoken
            var e = await _PollTokenGenerator.ExecuteGenerationByPollToken(args.PollToken);
            var keyCount = 0;
            if (e != null)
            {
                // Where(entity => entity.PublishingState > DateTime.Now.AddHours(-1).Date)
                keyCount = e.Keys.Count;
            }
            _DbContextProvider.SaveAndCommit();

            if (e != null && keyCount > 0)
            {
                return e;
            }

            throw new KeysUploadedNotValidException(e);
        }
    }

    public class KeysUploadedNotValidException : Exception
    {
        public KeyReleaseWorkflowState KeyReleaseWorkflowState;

        public KeysUploadedNotValidException() : base()
        {
        }

        public KeysUploadedNotValidException(KeyReleaseWorkflowState state) : base()
        {
            KeyReleaseWorkflowState = state;
        }
    }
}