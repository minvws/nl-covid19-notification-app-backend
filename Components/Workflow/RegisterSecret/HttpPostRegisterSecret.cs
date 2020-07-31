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
        private readonly IWorkflowStuff _WorkflowStuff;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly ILabConfirmationIdFormatter _LabConfirmationIdFormatter;

        public HttpPostRegisterSecret(ISecretWriter writer, ILogger<HttpPostRegisterSecret> logger, IWorkflowStuff workflowStuff, IUtcDateTimeProvider utcDateTimeProvider, ILabConfirmationIdFormatter labConfirmationIdFormatter)
        {
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _WorkflowStuff = workflowStuff ?? throw new ArgumentNullException(nameof(workflowStuff));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _LabConfirmationIdFormatter = labConfirmationIdFormatter ?? throw new ArgumentNullException(nameof(labConfirmationIdFormatter));
        }

        public async Task<IActionResult> Execute()
        {
            try
            {
                var entity = await _Writer.Execute();

                var result = new EnrollmentResponse
                {
                    ConfirmationKey = Convert.ToBase64String(entity.ConfirmationKey),
                    BucketId = Convert.ToBase64String(entity.BucketId),
                    //TODO remove formatting when spec is clarified to remove UI concern from data.
                    LabConfirmationId = _LabConfirmationIdFormatter.Format(entity.LabConfirmationId),
                    Validity = _WorkflowStuff.TimeToLiveSeconds(_UtcDateTimeProvider.Now(), entity.ValidUntil)
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.ToString());
                return new OkObjectResult(new EnrollmentResponse { Validity = -1 });
            }
        }
    }

    public interface ILabConfirmationIdFormatter
    {
        string Format(string value);
        string Parse(string value);
    }

    public class StandardLabConfirmationIdFormatter : ILabConfirmationIdFormatter
    {
        public string Format(string value) => $"{value.Substring(0, 3)}-{value.Substring(3, 3)}";
        public string Parse(string value) => value.Replace("-", string.Empty).Substring(6);
    }
}