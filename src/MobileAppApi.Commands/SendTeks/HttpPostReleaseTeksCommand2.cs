// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class HttpPostReleaseTeksCommand2
    {
        private readonly PostKeysLoggingExtensions _Logger;
        private readonly WorkflowDbContext _DbContext;

        private readonly IPostTeksValidator _KeyValidator;
        private readonly ITekWriter _Writer;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly ISignatureValidator _SignatureValidator;
        private readonly ITekListWorkflowFilter _TekListWorkflowFilter;
        private readonly IWorkflowConfig _WorkflowConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ITekValidPeriodFilter _TekApplicableWindowFilter;

        private PostTeksArgs _ArgsObject;
        private byte[] _BucketIdBytes;
        private byte[] _BodyBytes;

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
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _WorkflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
            _DbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _KeyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _SignatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
            _TekListWorkflowFilter = tekListWorkflowFilter ?? throw new ArgumentNullException(nameof(tekListWorkflowFilter));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _TekApplicableWindowFilter = tekApplicableWindowFilter ?? throw new ArgumentNullException(nameof(tekApplicableWindowFilter));
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
            _BodyBytes = body;

            if ((signature?.Length ?? 0) != UniversalConstants.PostKeysSignatureByteCount)
            {
                _Logger.WriteSignatureValidationFailed();
                return;
            }

            try
            {
                var argsJson = Encoding.UTF8.GetString(_BodyBytes);
                _ArgsObject = _JsonSerializer.Deserialize<PostTeksArgs>(argsJson);
            }
            catch (Exception e)
            {
                _Logger.WritePostBodyParsingFailed(e);
                return;
            }

            var base64Parser = new Base64();
            var base64ParserResult = base64Parser.TryParseAndValidate(_ArgsObject.BucketId, UniversalConstants.BucketIdByteCount);
            if (!base64ParserResult.Valid)
            {
                _Logger.WriteBucketIdParsingFailed(_ArgsObject.BucketId, base64ParserResult.Messages);
                return;
            }

            _BucketIdBytes = base64ParserResult.Item;

            var messages = _KeyValidator.Validate(_ArgsObject);
            if (messages.Length > 0)
            {
                _Logger.WriteTekValidationFailed(messages);
                return;
            }

            var teks = _ArgsObject.Keys.Select(Mapper.MapToTek).ToArray();
            foreach (var i in teks)
            {
                i.PublishAfter = _DateTimeProvider.Snapshot;
            }

            messages = new TekListDuplicateValidator().Validate(teks);
            if (messages.Length > 0)
            {
                _Logger.WriteTekDuplicatesFound(messages);
                return;
            }

            //Validation ends, filtering starts

            var filterResult = _TekApplicableWindowFilter.Execute(teks);
            _Logger.WriteApplicableWindowFilterResult(filterResult.Messages);
            teks = filterResult.Items;
            _Logger.WriteValidTekCount(teks.Length);

            var workflow = _DbContext
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .SingleOrDefault(x => x.BucketId == _BucketIdBytes);

            if (workflow == null)
            {
                _Logger.WriteBucketDoesNotExist(_ArgsObject.BucketId);
                return;
            }

            if (!_SignatureValidator.Valid(signature, workflow.ConfirmationKey, _BodyBytes))
            {
                _Logger.WriteSignatureInvalid(workflow.BucketId, signature);
                return;
            }

            var filterResults = _TekListWorkflowFilter.Filter(teks, workflow);
            _Logger.WriteWorkflowFilterResults(filterResults.Messages);
            _Logger.WriteValidTekCountSecondPass(teks.Length);

            //Run after the filter removes the existing TEKs from the args.
            var allTeks = workflow.Teks.Select(Mapper.MapToTek).Concat(filterResults.Items).ToArray();
            messages = new TekListDuplicateKeyDataValidator().Validate(allTeks);
            if (messages.Length > 0)
            {
                _Logger.WriteTekDuplicatesFoundWholeWorkflow(messages);
                return;
            }

            _Logger.WriteDbWriteStart();
            var writeArgs = new TekWriteArgs
            {
                WorkflowStateEntityEntity = workflow,
                NewItems = filterResults.Items
            };

            await _Writer.ExecuteAsync(writeArgs);
            _DbContext.SaveAndCommit();
            _Logger.WriteDbWriteCommitted();

            if (filterResults.Items.Length != 0)
                _Logger.WriteTekCountAdded(filterResults.Items.Length);
        }
    }
}