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
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class HttpPostReleaseTeksCommand
    {
        private readonly ILogger _logger;
        private readonly WorkflowDbContext _workflowDbContext;

        private readonly IPostTeksValidator _keyValidator;
        private readonly ITekWriter _writer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ISignatureValidator _signatureValidator;
        private readonly ITekListWorkflowFilter _tekListWorkflowFilter;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ITekValidPeriodFilter _tekApplicableWindowFilter;

        private PostTeksArgs _argsObject;
        private byte[] _bucketIdBytes;
        private byte[] _bodyBytes;

        public HttpPostReleaseTeksCommand(
            ILogger<HttpPostReleaseTeksCommand> logger,
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
            _workflowDbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
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
                _logger.LogInformation("Signature is null or incorrect length.");
                return;
            }

            try
            {
                var argsJson = Encoding.UTF8.GetString(_bodyBytes);
                _argsObject = _jsonSerializer.Deserialize<PostTeksArgs>(argsJson);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Error reading body");
                return;
            }

            var base64Parser = new Base64();
            var base64ParserResult = base64Parser.TryParseAndValidate(_argsObject.BucketId, UniversalConstants.BucketIdByteCount);
            if (!base64ParserResult.Valid)
            {
                _logger.LogInformation("BucketId failed validation - BucketId: {BucketId} Messages: \n{Messages}",
                    _argsObject.BucketId, string.Join("\n", base64ParserResult.Messages));
                return;
            }

            _bucketIdBytes = base64ParserResult.Item;

            var messages = _keyValidator.Validate(_argsObject);
            if (messages.Length > 0)
            {
                _logger.LogInformation("Tek failed validation - Messages: \n{Messages}", string.Join("\n", messages));
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
                _logger.LogInformation("Tek duplicates found - Messages: \n{Messages}", string.Join("\n", messages));
                return;
            }

            //Validation ends, filtering starts

            var filterResult = _tekApplicableWindowFilter.Execute(teks);

            if (filterResult.Messages.Length > 0)
            {
                _logger.LogInformation("Tek failed validation - Messages: \n{Messages}",
                    string.Join("\n", filterResult.Messages));
            }

            teks = filterResult.Items;
            _logger.LogInformation("TEKs remaining - Count: {ValidTekCount}.", teks.Length);

            var workflow = _workflowDbContext
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .SingleOrDefault(x => x.BucketId == _bucketIdBytes);

            if (workflow == null)
            {
                _logger.LogError("Bucket does not exist - Id: {BucketId}.", _argsObject.BucketId);
                return;
            }

            if (!_signatureValidator.Valid(signature, workflow.ConfirmationKey, _bodyBytes))
            {
                _logger.LogError("Signature not valid - Signature: {Signature} Bucket:{BucketId}",
                    Convert.ToBase64String(signature), Convert.ToBase64String(workflow.BucketId));

                return;
            }

            var filterResults = _tekListWorkflowFilter.Filter(teks, workflow);

            if (filterResults.Messages.Length > 0)
            {
                _logger.LogInformation("WriteWorkflowFilterResults: \n{Messages}.",
                    string.Join("\n", filterResults.Messages));
            }

            _logger.LogInformation("TEKs remaining in second pass - Count: {RemainingTekCount}.", teks.Length);

            //Run after the filter removes the existing TEKs from the args.
            var allTeks = workflow.Teks.Select(Mapper.MapToTek).Concat(filterResults.Items).ToArray();
            messages = new TekListDuplicateKeyDataValidator().Validate(allTeks);
            if (messages.Length > 0)
            {
                _logger.LogInformation("Tek duplicates found - Whole Workflow - Messages: \n{Messages}",
                    string.Join("\n", messages));
            }

            _logger.LogInformation("Teks added - Writing db.");

            var writeArgs = new TekWriteArgs
            {
                WorkflowStateEntityEntity = workflow,
                NewItems = filterResults.Items
            };

            _workflowDbContext.BeginTransaction();
            await _writer.ExecuteAsync(writeArgs);
            _workflowDbContext.SaveAndCommit();

            _logger.LogInformation("Teks added - Committed.");

            if (filterResults.Items.Length != 0)
            {
                _logger.LogInformation("Teks added - Count: {TeksAddedCount}.", filterResults.Items.Length);
            }
        }
    }
}
