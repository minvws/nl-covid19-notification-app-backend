// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iks.Protobuf;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksSendBatchCommand : BaseCommand
    {
        private readonly IksUploadService _iksUploadService;
        private readonly IksOutDbContext _iksOutboundDbContext;
        private List<IksOutEntity> _iksOutEntities;
        private readonly IIksSigner _signer;
        private readonly IBatchTagProvider _batchTagProvider;
        private readonly List<IksSendResult> _results = new List<IksSendResult>();
        private readonly HttpPostIksResult _lastResult;
        private readonly ILogger _logger;

        public IksSendBatchCommand(
            IksUploadService iksUploadService,
            IksOutDbContext iksOutboundDbContext,
            IIksSigner signer,
            IBatchTagProvider batchTagProvider,
            ILogger<IksSendBatchCommand> logger)
        {
            _iksUploadService = iksUploadService ?? throw new ArgumentNullException(nameof(iksUploadService));
            _iksOutboundDbContext = iksOutboundDbContext ?? throw new ArgumentNullException(nameof(iksOutboundDbContext));
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _batchTagProvider = batchTagProvider ?? throw new ArgumentNullException(nameof(batchTagProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            _iksOutEntities = _iksOutboundDbContext.Iks
                .Where(x => !x.Sent && x.CanRetry != false)
                .OrderBy(x => x.Created)
                .ThenBy(x => x.Qualifier)
                .Select(x => x)
                .ToList();

            foreach (var iksOutEntity in _iksOutEntities)
            {
                await ProcessOne(iksOutEntity);
            }

            return new IksSendBatchResult
            {
                Found = _iksOutEntities.Count,
                Sent = _results.ToArray()
            };
        }

        private byte[] SignDks(IksOutEntity item)
        {
            // Unpack
            var parser = new Google.Protobuf.MessageParser<DiagnosisKeyBatch>(() => new DiagnosisKeyBatch());
            var batch = parser.ParseFrom(item.Content);

            if (batch == null)
            {
                _logger.LogError("Batch does not exist in entity with Id: {EntityId}", item.Id);
                return null;
            }

            var efgsSerializer = new EfgsDiagnosisKeyBatchSerializer();

            //TODO: replace with call to HSM API; EfgsCmsSigner uses cert
            return _signer.GetSignature(efgsSerializer.Serialize(batch));
        }

        private async Task ProcessOne(IksOutEntity item)
        {
            var isNew = ProcessState.New.ToString().Equals(item.ProcessState);

            var signature = SignDks(item);

            // If the signature is null no batch was present.
            if (signature == null)
            {
                return;
            }

            var args = new IksSendCommandArgs
            {
                BatchTag = _batchTagProvider.Create(item.Content),
                Content = item.Content,
                Signature = signature
            };

            var httpPostIksResult = await SendOne(args, item);
            // Note: EFGS returns Created or OK on creation
            if (!isNew && httpPostIksResult.Exception)
            {
                item.RetryCount++;
                _logger.LogError(
                    "Batch does not exist in entity with Id: {EntityId}, retry count: {EntityRetryCount}",
                    item.Id, item.RetryCount);
            }

            item.Sent = httpPostIksResult.HttpResponseCode == HttpStatusCode.Created || httpPostIksResult.HttpResponseCode == HttpStatusCode.OK;
            item.Error = !item.Sent;
            await _iksOutboundDbContext.SaveChangesAsync();

            var iksSendResult = new IksSendResult
            {
                Exception = httpPostIksResult.Exception,
                StatusCode = httpPostIksResult?.HttpResponseCode
            };

            _results.Add(iksSendResult);
        }

        /// <summary>
        /// Pass/Fail
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task<HttpPostIksResult> SendOne(IksSendCommandArgs args, IksOutEntity item)
        {
            var result = await _iksUploadService.ExecuteAsync(args);

            if (result != null)
            {
                switch (result.HttpResponseCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        item.CanRetry = false; // No retry needed when sent successfully
                        item.ProcessState = ProcessState.Sent.ToString();
                        _logger.LogInformation("EFGS: Success.");
                        break;
                    case HttpStatusCode.MultiStatus:
                        item.CanRetry = false; // Set Retry to false. After fix, set the value to true manually
                        _logger.LogWarning(
                            "HTTP 207 Multi-Status returned (either duplicates in the data or EFGS server error). Warnings {ResponseContent}",
                            result.Content);
                        item.ProcessState = ProcessState.Invalid.ToString();
                        break;
                    case HttpStatusCode.Forbidden:
                        item.CanRetry = false; // Set Retry to false. After fix, set the value to true manually
                        item.ProcessState = ProcessState.Invalid.ToString(); // Adjust State to Invalid
                        _logger.LogError("EFGS: Invalid/missing certificates.");
                        break;
                    case HttpStatusCode.RequestEntityTooLarge:
                        item.CanRetry = false; // Set Retry to false. After fix, set the value to true manually
                        item.ProcessState = ProcessState.Invalid.ToString(); // Adjust State to Invalid
                        _logger.LogError("EFGS: Data already exists (Http: Request Entity too large).");
                        break;
                    case HttpStatusCode.NotAcceptable:
                        item.CanRetry = false; // Set Retry to false. After fix, set the value to true manually
                        item.ProcessState = ProcessState.Skipped.ToString(); // Adjust State to Skipped
                        _logger.LogError("EFGS:  Data format or content is not valid.");
                        break;
                    case HttpStatusCode.BadRequest:
                        item.CanRetry = false; // Set Retry to false. After fix, set the value to true manually
                        item.ProcessState = ProcessState.Failed.ToString(); // Adjust State to Failed
                        _logger.LogError("EFGS: Invalid request (either errors in the data or an invalid signature).");
                        break;
                    case HttpStatusCode.InternalServerError:
                        item.CanRetry = true;
                        item.ProcessState = ProcessState.Failed.ToString(); // Adjust State to Failed 
                        _logger.LogError("EFGS: Not able to write data. Retry please.");
                        break;
                    default:
                        item.CanRetry = true;
                        item.ProcessState = ProcessState.Failed.ToString(); // Adjust State to Failed
                        _logger.LogError("Unknown error: {HttpResponseCode}.",
                            result.HttpResponseCode?.ToString() ?? "No HttpStatusCode received");
                        break;
                }
            }

            return result;
        }
    }
}
