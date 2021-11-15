// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iks.Protobuf;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksSendBatchCommand : BaseCommand
    {
        private readonly HttpPostIksCommand _httpPostIksCommand;
        private readonly IksOutDbContext _iksOutboundDbContext;
        private List<IksOutEntity> _todo;
        private readonly IIksSigner _signer;
        private readonly IBatchTagProvider _batchTagProvider;
        private readonly List<IksSendResult> _results = new List<IksSendResult>();
        private HttpPostIksResult _lastResult;
        private readonly IksUploaderLoggingExtensions _logger;

        public IksSendBatchCommand(
            IksOutDbContext iksOutboundDbContext,
            HttpPostIksCommand httpPostIksCommand,
            IIksSigner signer,
            IBatchTagProvider batchTagProvider,
            IksUploaderLoggingExtensions logger)
        {
            _httpPostIksCommand = httpPostIksCommand ?? throw new ArgumentNullException(nameof(httpPostIksCommand));
            _iksOutboundDbContext = iksOutboundDbContext ?? throw new ArgumentNullException(nameof(iksOutboundDbContext));
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _batchTagProvider = batchTagProvider ?? throw new ArgumentNullException(nameof(batchTagProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            _todo = _iksOutboundDbContext.Iks
                .Where(x => !x.Sent && !x.Error)
                .OrderBy(x => x.Created)
                .ThenBy(x => x.Qualifier)
                .Select(x => x)
                .ToList();

            foreach (var iksOutEntity in _todo)
            {
                await ProcessOne(iksOutEntity);
            }

            return new IksSendBatchResult
            {
                Found = _todo.Count,
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
                _logger.WriteBatchNotExistInEntity(item);
                return null;
            }

            var efgsSerializer = new EfgsDiagnosisKeyBatchSerializer();

            return _signer.GetSignature(efgsSerializer.Serialize(batch));
        }

        private async Task ProcessOne(IksOutEntity item)
        {
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

            await SendOne(args);

            var result = new IksSendResult
            {
                Exception = _lastResult.Exception,
                StatusCode = _lastResult?.HttpResponseCode
            };

            _results.Add(result);

            // Note: EFGS returns Created or OK on creation
            item.Sent = _lastResult?.HttpResponseCode == HttpStatusCode.Created;
            item.Error = !item.Sent;

            await _iksOutboundDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Pass/Fail
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task SendOne(IksSendCommandArgs args)
        {
            // NOTE: no retry here
            var result = await _httpPostIksCommand.ExecuteAsync(args);

            _lastResult = result;

            if (result != null)
            {
                switch (result.HttpResponseCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        _logger.WriteResponseSuccess();
                        return;
                    case HttpStatusCode.MultiStatus:
                        _logger.WriteResponseWithWarnings(result.Content);
                        return;
                    case HttpStatusCode.BadRequest:
                        _logger.WriteResponseBadRequest();
                        break;
                    case HttpStatusCode.Forbidden:
                        _logger.WriteResponseForbidden();
                        break;
                    case HttpStatusCode.NotAcceptable:
                        _logger.WriteResponseNotAcceptable();
                        break;
                    case HttpStatusCode.RequestEntityTooLarge:
                        _logger.WriteResponseRequestTooLarge();
                        break;
                    case HttpStatusCode.InternalServerError:
                        _logger.WriteResponseInternalServerError();
                        break;
                    default:
                        _logger.WriteResponseUnknownError(result.HttpResponseCode);
                        break;
                }
            }
        }
    }
}
