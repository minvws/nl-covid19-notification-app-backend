// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class HttpPostReleaseTeksCommand2
    {
        private readonly PostKeysLoggingExtensions _logger;
        private readonly WorkflowDbContext _dbContext;

        private readonly IPostTeksValidator _keyValidator;
        private readonly ITekWriter _writer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ISignatureValidator _signatureValidator;
        private readonly ITekListWorkflowFilter _tekListWorkflowFilter;
        private readonly IWorkflowConfig _workflowConfig;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ITekValidPeriodFilter _tekApplicableWindowFilter;

        private PostTeksArgs _argsObject;
        private byte[] _bucketIdBytes;
        private byte[] _bodyBytes;

        public HttpPostReleaseTeksCommand2(
            PostKeysLoggingExtensions logger,
            IWorkflowConfig workflowConfig,
            WorkflowDbContext dbContextProvider,
            IPostTeksValidator keyValidator,
            ITekWriter writer,
            IJsonSerializer jsonSerializer,
            ISignatureValidator signatureValidator,
            ITekListWorkflowFilter tekListWorkflowFilter,
            IUtcDateTimeProvider dateTimeProvider,
            ITekValidPeriodFilter tekApplicableWindowFilter
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
            _dbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _keyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _signatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
            _tekListWorkflowFilter = tekListWorkflowFilter ?? throw new ArgumentNullException(nameof(tekListWorkflowFilter));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _tekApplicableWindowFilter = tekApplicableWindowFilter ?? throw new ArgumentNullException(nameof(tekApplicableWindowFilter));
        }

        public async Task<IActionResult> ExecuteAsync(byte[] signature, HttpRequest request)
        {
            await using var mem = new MemoryStream();
            await request.Body.CopyToAsync(mem);
            await InnerExecuteAsync(signature, mem.ToArray());
            return new OkResult();
        }

        private async Task InnerExecuteAsync(byte[] signature, byte[] body)
        {
            _bodyBytes = body;

            if ((signature?.Length ?? 0) != UniversalConstants.PostKeysSignatureByteCount)
            {
                _logger.WriteSignatureValidationFailed();
                return;
            }

            try
            {
                var argsJson = Encoding.UTF8.GetString(_bodyBytes);
                _argsObject = _jsonSerializer.Deserialize<PostTeksArgs>(argsJson);
            }
            catch (Exception e)
            {
                _logger.WritePostBodyParsingFailed(e);
                return;
            }

            var base64Parser = new Base64();
            var base64ParserResult = base64Parser.TryParseAndValidate(_argsObject.BucketId, UniversalConstants.BucketIdByteCount);
            if (!base64ParserResult.Valid)
            {
                _logger.WriteBucketIdParsingFailed(_argsObject.BucketId, base64ParserResult.Messages);
                return;
            }

            _bucketIdBytes = base64ParserResult.Item;

            var messages = _keyValidator.Validate(_argsObject);
            if (messages.Length > 0)
            {
                _logger.WriteTekValidationFailed(messages);
                return;
            }

            var teks = _argsObject.Keys.Select(Mapper.MapToTek).ToArray();
            foreach (var i in teks)
            {
                i.PublishAfter = _dateTimeProvider.Snapshot;
            }

            messages = new TekListDuplicateValidator().Validate(teks);
            if (messages.Length > 0)
            {
                _logger.WriteTekDuplicatesFound(messages);
                return;
            }

            //Validation ends, filtering starts

            var filterResult = _tekApplicableWindowFilter.Execute(teks);
            _logger.WriteApplicableWindowFilterResult(filterResult.Messages);
            teks = filterResult.Items;
            _logger.WriteValidTekCount(teks.Length);

            var workflow = _dbContext
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .SingleOrDefault(x => x.BucketId == _bucketIdBytes);

            if (workflow == null)
            {
                _logger.WriteBucketDoesNotExist(_argsObject.BucketId);
                return;
            }

            if (!_signatureValidator.Valid(signature, workflow.ConfirmationKey, _bodyBytes))
            {
                _logger.WriteSignatureInvalid(workflow.BucketId, signature);
                return;
            }

            var filterResults = _tekListWorkflowFilter.Filter(teks, workflow);
            _logger.WriteWorkflowFilterResults(filterResults.Messages);
            _logger.WriteValidTekCountSecondPass(teks.Length);

            //Run after the filter removes the existing TEKs from the args.
            var allTeks = workflow.Teks.Select(Mapper.MapToTek).Concat(filterResults.Items).ToArray();
            messages = new TekListDuplicateKeyDataValidator().Validate(allTeks);
            if (messages.Length > 0)
            {
                _logger.WriteTekDuplicatesFoundWholeWorkflow(messages);
                return;
            }

            _logger.WriteDbWriteStart();
            var writeArgs = new TekWriteArgs
            {
                WorkflowStateEntityEntity = workflow,
                NewItems = filterResults.Items
            };

            await _writer.ExecuteAsync(writeArgs);
            _dbContext.SaveAndCommit();
            _logger.WriteDbWriteCommitted();

            if (filterResults.Items.Length != 0)
                _logger.WriteTekCountAdded(filterResults.Items.Length);
        }
    }
}
