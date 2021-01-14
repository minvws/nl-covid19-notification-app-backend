﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class HttpPostRegisterSecret
    {
        private readonly ISecretWriter _Writer;
        private readonly RegisterSecretLoggingExtensions _Logger;
        private readonly IWorkflowTime _WorkflowTime;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly ILabConfirmationIdFormatter _LabConfirmationIdFormatter;
        private readonly IDecoyTimeCalculator _DecoyTimeCalculator;
        private Stopwatch _Stopwatch;

        public HttpPostRegisterSecret(
            ISecretWriter writer,
            RegisterSecretLoggingExtensions logger,
            IWorkflowTime workflowTime, 
            IUtcDateTimeProvider utcDateTimeProvider,
            ILabConfirmationIdFormatter labConfirmationIdFormatter,
            IDecoyTimeCalculator decoyTimeCalculator)
        {
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _WorkflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _LabConfirmationIdFormatter = labConfirmationIdFormatter ?? throw new ArgumentNullException(nameof(labConfirmationIdFormatter));
            _DecoyTimeCalculator = decoyTimeCalculator ?? throw new ArgumentNullException(nameof(decoyTimeCalculator));
        }

        public async Task<IActionResult> ExecuteAsync()
        {
            try
            {
                _Stopwatch = new Stopwatch();
                _Stopwatch.Start();

                var entity = await _Writer.ExecuteAsync();

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
                _Stopwatch.Stop();
                _DecoyTimeCalculator.RegisterTime(_Stopwatch.ElapsedMilliseconds);
                _Stopwatch.Reset();
            }
        }
    }
}