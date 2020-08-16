// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class HttpPostReleaseTeksCommand2
    {
        private readonly ILogger<HttpPostReleaseTeksCommand2> _Logger;
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


        public HttpPostReleaseTeksCommand2(ILogger<HttpPostReleaseTeksCommand2> logger, IWorkflowConfig workflowConfig,
            WorkflowDbContext dbContextProvider, IPostTeksValidator keyValidator, ITekWriter writer,
            IJsonSerializer jsonSerializer, ISignatureValidator signatureValidator,
            ITekListWorkflowFilter tekListWorkflowFilter, IUtcDateTimeProvider dateTimeProvider, ITekValidPeriodFilter tekApplicableWindowFilter)
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

        public async Task<IActionResult> Execute(byte[] signature, HttpRequest request)
        {
            await using var mem = new MemoryStream();
            await request.Body.CopyToAsync(mem);
            await InnerExecute(signature, mem.ToArray());
            return new OkResult();
        }

        private async Task InnerExecute(byte[] signature, byte[] body)
        {
            _Logger.LogDebug("Signature received: {Signature}", signature);

            _BodyBytes = body;

            if ((signature?.Length ?? 0) != _WorkflowConfig.PostKeysSignatureLength) //TODO const
            {
                _Logger.LogError("Signature is null or incorrect length.");
                return;
            }

            try
            {
                var argsJson = Encoding.UTF8.GetString(_BodyBytes);
                _Logger.LogDebug("Body -\n{ArgsJson}.", argsJson);
                _ArgsObject = _JsonSerializer.Deserialize<PostTeksArgs>(argsJson);
            }
            catch (Exception e)
            {
                //TODO: check if you want to use Serilog's Exception logging, or just use ToString
                //i.e., _logger.LogError(e, "Error reading body");
                _Logger.LogError("Error reading body -\n{E}", e);
                return;
            }

            var base64Parser = new Base64();
            var base64ParserResult = base64Parser.TryParseAndValidate(_ArgsObject.BucketId, _WorkflowConfig.BucketIdLength);
            if (!base64ParserResult.Valid)
            {
                _Logger.LogValidationMessages(base64ParserResult.Messages.Select(x => $"BuckedId - {x}").ToArray());
                return;
            }

            _BucketIdBytes = base64ParserResult.Item;

            if (_Logger.LogValidationMessages(_KeyValidator.Validate(_ArgsObject)))
                return;

            var teks = _ArgsObject.Keys.Select(Mapper.MapToTek).ToArray();

            foreach (var i in teks)
            {
                i.PublishAfter = _DateTimeProvider.Snapshot;
            }

            if (_Logger.LogValidationMessages(new TekListDuplicateValidator().Validate(teks)))
                return;

            //Validation ends, filtering starts

            var filterResult = _TekApplicableWindowFilter.Execute(teks);
            teks = filterResult.Items;
            _Logger.LogFilterMessages(filterResult.Messages);
            _Logger.LogInformation("TEKs remaining - Count:{count}.", teks.Length);

            var workflow = _DbContext
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .SingleOrDefault(x => x.BucketId == _BucketIdBytes);

            if (workflow == null)
            {
                _Logger.LogError("Workflow does not exist - {BucketId}.", _ArgsObject.BucketId);
                return;
            }

            if (!_SignatureValidator.Valid(signature, workflow.ConfirmationKey, _BodyBytes))
            {
                _Logger.LogError("Signature not valid: {Signature}", signature);
                return;
            }

            var filterResults = _TekListWorkflowFilter.Filter(teks, workflow);
            _Logger.LogFilterMessages(filterResults.Messages);
            _Logger.LogInformation("TEKs remaining - Count:{count}.", teks.Length);

            //Run after the filter removes the existing TEKs from the args.
            var allTeks = workflow.Teks.Select(Mapper.MapToTek).Concat(filterResults.Items).ToArray();
            if (_Logger.LogValidationMessages(new TekListDuplicateKeyDataValidator().Validate(allTeks)))
                return;

            _Logger.LogDebug("Writing.");
            var writeArgs = new TekWriteArgs
            {
                WorkflowStateEntityEntity = workflow,
                NewItems = filterResults.Items
            };

            await _Writer.Execute(writeArgs);
            _DbContext.SaveAndCommit();
            _Logger.LogDebug("Committed.");

            if (filterResults.Items.Length != 0)
            {
                _Logger.LogInformation("Teks added - Count:{FilterResultsLength}.", filterResults.Items.Length);
            }
        }
    }
}