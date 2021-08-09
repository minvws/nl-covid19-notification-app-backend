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
                throw new Exception("TODO Something went wrong");
            }

            var efgsSerializer = new EfgsDiagnosisKeyBatchSerializer();

            return _signer.GetSignature(efgsSerializer.Serialize(batch));
        }

        private async Task ProcessOne(IksOutEntity item)
        {
            //var item = await _iksOutboundDbContext.Iks.f(x => x.Id == i);

            var args = new IksSendCommandArgs
            {
                BatchTag = _batchTagProvider.Create(item.Content),
                Content = item.Content,
                Signature = SignDks(item)
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

            // TODO: Implement a state machine for batches; this is useful around error cases.
            // * Re-try for selected states.
            // * For data errors, end state with invalid (initially).
            // * Allow for manual fixing of data errors with a special retry state?
            //

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

            // TODO: handle the return types
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

            // TODO for Production Quality Code:
            //
            // Handle the error codes like this:
            //
            // Auto-retry: InternalServerError, ANY undefined error
            // Fix config then retry: BadRequest, Forbidden
            // Fix file then retry: NotAcceptable
            // Skip: NotAcceptable
            //
            // Also: consider splitting this file up into a class which makes the calls, and a class
            // which handles the workflow described above.
            //
            // The table IksOut will gain the fields: State, RetryCount, Retry flag
            // 
            // Code modified to include anything tagged with the Retry flag again.
            //
            // We must also define a State enumeration with a logical set of states as per the error handling.
            // State diagram is helpful here (TODO: Ryan)
            //
            // Basically we must be able to manually trigger retries for any data errors and configuration errors, have automatic retry for
            // transient errors. Ideally driven by some kind of portal, but at first it will be DB tinkering.
            //
            // For states:
            //
            // States: New, Failed, Sent (ended successfully), Skipped (ended unsuccessfully)
            // Failed states (combined Efgs and our own errors):
            //    EfgsInvalidSignature, EfgsInvalidCertificate, EfgsInvalidContent, EfgsDuplicateContent, EfgsUnavailable, EfgsUndefined
            //    UnableToConnect (when we can't connect to efgs), Unknown (catch-all for any other errors)
            //
            // I think that it's cleaner to split the states into State and FailedState; the latter being more detailed states for failures.

        }
    }
}
