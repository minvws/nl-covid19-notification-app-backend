// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class HttpPostRegisterSecret
    {
        private readonly ISecretWriter _Writer;
        private readonly ILogger _Logger;
        private readonly IWorkflowTime _WorkflowTime;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly ILabConfirmationIdFormatter _LabConfirmationIdFormatter;

        public HttpPostRegisterSecret(ISecretWriter writer, ILogger<HttpPostRegisterSecret> logger, IWorkflowTime workflowTime, IUtcDateTimeProvider utcDateTimeProvider, ILabConfirmationIdFormatter labConfirmationIdFormatter)
        {
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _WorkflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _LabConfirmationIdFormatter = labConfirmationIdFormatter ?? throw new ArgumentNullException(nameof(labConfirmationIdFormatter));
        }

        public async Task<IActionResult> Execute()
        {
            _Logger.WriteStart();
            try
            {
                var entity = await _Writer.Execute();

                var result = new EnrollmentResponse
                {
                    ConfirmationKey = Convert.ToBase64String(entity.ConfirmationKey),
                    BucketId = Convert.ToBase64String(entity.BucketId),
                    LabConfirmationId = _LabConfirmationIdFormatter.Format(entity.LabConfirmationId), //Architects choice to use UI format in response.
                    Validity = _WorkflowTime.TimeToLiveSeconds(_UtcDateTimeProvider.Snapshot, entity.ValidUntil)
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _Logger.WriteFailed(ex);
                return new OkObjectResult(new EnrollmentResponse { Validity = -1 });
            }
            finally
            {
                _Logger.WriteFinished();
            }
        }
    }
}