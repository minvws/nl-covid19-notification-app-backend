// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
//{
//    [Obsolete("Use filter approach.")]
//    public class HttpPostReleaseTeksCommand
//    {
//        private readonly ILogger _Logger;
//        private readonly WorkflowDbContext _DbContextProvider;

//        private readonly IPostTeksValidator _KeyValidator;
//        private readonly ITekWriter _Writer;
//        private readonly IJsonSerializer _JsonSerializer;
//        private readonly ISignatureValidator _SignatureValidator;

//        private readonly INewTeksValidator _NewTeksValidator;

//        private PostTeksArgs _ArgsObject;
//        private byte[] _BucketIdBytes;
//        private byte[] _BodyBytes;

//        public HttpPostReleaseTeksCommand(IPostTeksValidator keyValidator, ITekWriter writer, WorkflowDbContext dbContextProvider, IJsonSerializer jsonSerializer, INewTeksValidator newTeksValidator, ILogger<HttpPostReleaseTeksCommand> logger, ISignatureValidator signatureValidator)
//        {
//            _KeyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
//            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
//            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
//            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
//            _NewTeksValidator = newTeksValidator ?? throw new ArgumentNullException(nameof(newTeksValidator));
//            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            _SignatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
//        }

//        public async Task<IActionResult> Execute(byte[] signature, HttpRequest request)
//        {
//            await using var mem = new MemoryStream();
//            await request.Body.CopyToAsync(mem);
//            await InnerExecute(signature, mem.ToArray());
//            return new OkResult();
//        }

//        async Task InnerExecute(byte[] signature, byte[] body)
//        {
//            _BodyBytes = body;

//            if ((signature?.Length ?? 0) != 32) //TODO const
//            {
//                _Logger.LogError("Signature is null or incorrect length.");
//                return;
//            }

//            try
//            {
//                var argsJson = Encoding.UTF8.GetString(_BodyBytes);
//                _Logger.LogDebug("Body -\n{ArgsJson}.", argsJson);
//                _ArgsObject = _JsonSerializer.Deserialize<PostTeksArgs>(argsJson);
//            }
//            catch (Exception e)
//            {
//                //TODO: check if you want to use Serilog's Exception logging, or just use ToString
//                //i.e., _logger.LogError(e, "Error reading body");
//                _Logger.LogError("Error reading body -\n{E}", e);
//                return;
//            }

//            try
//            {
//                //NB there is a Try but Span<byte> cannot be used in async functions
//                _BucketIdBytes = Convert.FromBase64String(_ArgsObject.BucketId);
//            }
//            catch (FormatException e)
//            {
//                //TODO: check if you want to use Serilog's Exception logging, or just use ToString
//                //i.e., _logger.LogError(e, "Error parsing BucketId");
//                _Logger.LogError("Error parsing BucketId -\n{E}", e);
//                return;
//            }

//            if (_Logger.LogValidationMessages(_KeyValidator.Validate(_ArgsObject)))
//                return;

//            var teks = _ArgsObject.Keys.Select(Mapper.MapToTek).ToArray();
//            if (_Logger.LogValidationMessages(new TekListDuplicateValidator().Validate(teks)))
//                return;

//            var workflow = _DbContextProvider
//                .KeyReleaseWorkflowStates
//                .Include(x => x.Teks)
//                .FirstOrDefault(x => x.BucketId == _BucketIdBytes);

//            if (workflow == null)
//            {
//                _Logger.LogError("Workflow does not exist - {BucketId}.", _ArgsObject.BucketId);
//                return;
//            }

//            if (!_SignatureValidator.Valid(signature, workflow.ConfirmationKey, _BodyBytes))
//            {
//                _Logger.LogError("Signature not valid.");
//                return;
//            }

//            if (_Logger.LogValidationMessages(_NewTeksValidator.Validate(teks, workflow)))
//                return;

//            _Logger.LogDebug("Writing.");
//            var writeArgs = new TekWriteArgs
//            {
//                WorkflowStateEntityEntity = workflow,
//                NewItems = teks
//            };
//            await _Writer.Execute(writeArgs);
//            _DbContextProvider.SaveAndCommit();
//            _Logger.LogDebug("Committed.");
//        }
//    }
//}