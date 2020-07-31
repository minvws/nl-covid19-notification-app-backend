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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class HttpPostReleaseTeksCommand2
    {
        private readonly ILogger _Logger;
        private readonly WorkflowDbContext _DbContextProvider;

        private readonly IPostTeksValidator _KeyValidator;
        private readonly ITekWriter _Writer;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly ISignatureValidator _SignatureValidator;
        private readonly ITekListWorkflowFilter _TekListWorkflowFilter;

        private PostTeksArgs _ArgsObject;
        private byte[] _BucketIdBytes;
        private byte[] _BodyBytes;
        //ILogger<HttpPostReleaseTeksCommand2> logger

        public HttpPostReleaseTeksCommand2(ILogger<HttpPostReleaseTeksCommand2> logger, WorkflowDbContext dbContextProvider, IPostTeksValidator keyValidator, ITekWriter writer, IJsonSerializer jsonSerializer, ISignatureValidator signatureValidator, ITekListWorkflowFilter tekListWorkflowFilter)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _KeyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _SignatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
            _TekListWorkflowFilter = tekListWorkflowFilter ?? throw new ArgumentNullException(nameof(tekListWorkflowFilter));
        }

        public async Task<IActionResult> Execute(byte[] signature, HttpRequest request)
        {
            await using var mem = new MemoryStream();
            await request.Body.CopyToAsync(mem);
            await InnerExecute(signature, mem.ToArray());
            return new OkResult();
        }

        async Task InnerExecute(byte[] signature, byte[] body)
        {
            _BodyBytes = body;

            if ((signature?.Length ?? 0) != 32) //TODO const
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

            try
            {
                //NB there is a Try but Span<byte> cannot be used in async functions
                _BucketIdBytes = Convert.FromBase64String(_ArgsObject.BucketId);
            }
            catch (FormatException e)
            {
                //TODO: check if you want to use Serilog's Exception logging, or just use ToString
                //i.e., _logger.LogError(e, "Error parsing BucketId");
                _Logger.LogError("Error parsing BucketId -\n{E}", e);
                return;
            }

            if (_Logger.LogValidationMessages(_KeyValidator.Validate(_ArgsObject)))
                return;

            var teks = _ArgsObject.Keys.Select(Mapper.MapToTek).ToArray();
            if (_Logger.LogValidationMessages(new TekListDuplicateValidator().Validate(teks)))
                return;

            var workflow = _DbContextProvider
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefault(x => x.BucketId == _BucketIdBytes);

            if (workflow == null)
            {
                _Logger.LogError("Workflow does not exist - {BucketId}.", _ArgsObject.BucketId);
                return;
            }

            if (!_SignatureValidator.Valid(signature, workflow.ConfirmationKey, _BodyBytes))
            {
                _Logger.LogError("Signature not valid.");
                return;
            }

            var filterResults = _TekListWorkflowFilter.Validate(teks, workflow);
            _Logger.LogValidationMessages(filterResults.Messages);

            if (filterResults.Items.Length == 0)
            {
                _Logger.LogInformation("No teks survived the workflow filter.");
                return;
            }

            _Logger.LogDebug("Writing.");
            var writeArgs = new TekWriteArgs
            {
                WorkflowStateEntityEntity = workflow,
                NewItems = filterResults.Items
            };
            await _Writer.Execute(writeArgs);
            _DbContextProvider.SaveAndCommit();
            _Logger.LogDebug("Committed.");

            if (filterResults.Items.Length != 0)
            {
                _Logger.LogInformation("Teks added - Count:{FilterResultsLength}.", filterResults.Items.Length);
            }
        }
    }
}