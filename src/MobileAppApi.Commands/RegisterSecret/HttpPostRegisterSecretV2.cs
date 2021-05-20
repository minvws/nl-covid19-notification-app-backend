// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class HttpPostRegisterSecretV2
    {
        private readonly TekReleaseWorkflowStateCreateV2 _writer;
        private readonly RegisterSecretLoggingExtensionsV2 _logger;
        private readonly IWorkflowTime _workflowTime;
        private readonly IUtcDateTimeProvider _utcDateTimeProvider;

        public HttpPostRegisterSecretV2(
            TekReleaseWorkflowStateCreateV2 writer,
            RegisterSecretLoggingExtensionsV2 logger,
            IWorkflowTime workflowTime,
            IUtcDateTimeProvider utcDateTimeProvider
            )
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _utcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
        }

        public async Task<IActionResult> ExecuteAsync()
        {
            _logger.WriteStartSecret();

            try
            {
                var entity = await _writer.ExecuteAsync();

                var result = new EnrollmentResponseV2
                {
                    ConfirmationKey = Convert.ToBase64String(entity.ConfirmationKey),
                    BucketId = Convert.ToBase64String(entity.BucketId),
                    GGDKey = entity.GGDKey,
                    Validity = _workflowTime.TimeToLiveSeconds(_utcDateTimeProvider.Snapshot, entity.ValidUntil)
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.WriteFailed(ex);
                return new OkObjectResult(new EnrollmentResponseV2 { Validity = -1 });
            }
            finally
            {
                _logger.WriteFinished();
            }
        }
    }
}
