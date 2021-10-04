// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication
{
    public class PublishTekCommand : BaseCommand
    {
        private readonly WorkflowDbContext _workflowDb;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;

        private int _attemptCount;
        private const int AttemptCountMax = 5;

        public PublishTekCommand(WorkflowDbContext workflowDb, IUtcDateTimeProvider dateTimeProvider, ILogger<PublishTekCommand> logger)
        {
            _workflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Assume args validated
        /// </summary>
        /// <param name="parameters"> Containing args && isOriginPortal values</param>
        /// <param name="args">The PubTEK values</param>
        /// <param name="isOriginPortal">True if the Origin of the args is the Icc portal, otherwise false</param>
        /// <returns>True if succeeded, otherwise false</returns>
        public override async Task<ICommandResult> ExecuteAsync(IParameters parameters)
        {
            var args = ((Parameters)parameters).PublishTekArgs;
            var isOriginPortal = ((Parameters)parameters).IsOriginPortal;

            // Defensive check for PublishTekArgs being null
            if (((Parameters)parameters).PublishTekArgs == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            // Retrieve the matching PubTEK value with all TEK's from the database
            var wf = await _workflowDb
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(x => x.GGDKey == args.GGDKey || args.GGDKey.StartsWith(x.LabConfirmationId));

            // If no PubTEK value is found the process should be ended. The PubTEK key does not exist or is already processed/published.
            if (wf == null)
            {
                _logger.WriteKeyReleaseWorkflowStateNotFound(args.GGDKey);
                return new CommandResult{ HasErrors = true };
            }

            wf.AuthorisedByCaregiver = _dateTimeProvider.Snapshot;
            wf.LabConfirmationId = null; //Clear from usable key range
            wf.GGDKey = null; //Clear from usable key range
            wf.StartDateOfTekInclusion = args.SubjectHasSymptoms ? args.DateOfSymptomsOnset : args.DateOfTest; // The date is currently a StartDateOfTekInclusion or Date of Test. The system lacks having 2 date variants so the existing StartDateOfTekInclusion will hold either
            wf.IsSymptomatic = args.SubjectHasSymptoms ? InfectiousPeriodType.Symptomatic : InfectiousPeriodType.Asymptomatic;

            var success = await PublishTek(wf);

            if (success)
            {
                _logger.LogInformation($"GGDKey {wf.GGDKey} authorized.");
            }

            return new CommandResult{ HasErrors = false };
        }

        private async Task<bool> PublishTek(TekReleaseWorkflowStateEntity workflowStateEntity)
        {
            _logger.WriteWritingPublishTek();

            var success = await WriteAttempt(workflowStateEntity);
            while (!success)
            {
                success = await WriteAttempt(workflowStateEntity);
            }

            return success;
        }
        private async Task<bool> WriteAttempt(TekReleaseWorkflowStateEntity workflowStateEntity)
        {
            if (++_attemptCount > AttemptCountMax)
            {
                throw new InvalidOperationException("Maximum attempts reached.");
            }

            if (_attemptCount > 1)
            {
                _logger.WriteDuplicatePollTokenFound(_attemptCount);
            }

            try
            {
                await _workflowDb.SaveChangesAsync();
                _logger.WritePollTokenCommit();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (CanRetry(ex))
                {
                    return false;
                }

                throw;
            }
        }

        //E.g. Microsoft.Data.SqlClient.SqlException(0x80131904):
        //Cannot insert duplicate key row in object 'dbo.TekReleaseWorkflowState'
        //with unique index 'IX_TekReleaseWorkflowState_BucketId'.The duplicate key value is (blah blah).
        private bool CanRetry(DbUpdateException ex)
        {
            if (!(ex.InnerException is SqlException sqlEx))
            {
                return false;
            }

            var errors = new SqlError[sqlEx.Errors.Count];
            sqlEx.Errors.CopyTo(errors, 0);

            return errors.Any(x =>
                x.Number == 2601
                && x.Message.Contains("TekReleaseWorkflowState")
                && x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.GGDKey))); // TODO: Rewritten code after removing the update on PollToken. Error can be less relative at this moment. 
        }

        public new class Parameters : IParameters
        {
            public PublishTekArgs PublishTekArgs { get; set; }
            public bool IsOriginPortal { get; set; }
        }
    }
}
