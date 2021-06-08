// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class AuthorizeLabConfirmationIdCommand
    {
        private readonly WorkflowDbContext _workflowDb;
        private readonly ILogger _logger;

        public AuthorizeLabConfirmationIdCommand(WorkflowDbContext workflowDb, ILogger<AuthorisationWriterCommand> logger)
        {
            _workflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Assume args validated
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteAsync(AuthorisationArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var wf = await _workflowDb
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
            {
                _logger.WriteKeyReleaseWorkflowStateNotFound(args.LabConfirmationId);
                return false;
            }

            _logger.LogInformation("LabConfirmationId {LabConfirmationId} authorized.", wf.LabConfirmationId);

            return true;
        }
    }
}
