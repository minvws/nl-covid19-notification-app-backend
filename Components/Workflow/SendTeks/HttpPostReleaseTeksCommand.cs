// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Buffers.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class HttpPostReleaseTeksCommand
    {
        private readonly IReleaseTeksValidator _KeyValidator;
        private readonly ISignatureValidator _SignatureValidator;
        private readonly ITekWriter _Writer;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly ILogger _Logger;

        public HttpPostReleaseTeksCommand(IReleaseTeksValidator keyValidator, ISignatureValidator signatureValidator, ITekWriter writer, WorkflowDbContext dbContextProvider, IJsonSerializer jsonSerializer, ILogger<HttpPostReleaseTeksCommand> logger)
        {
            _KeyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
            _SignatureValidator = signatureValidator ?? throw new ArgumentNullException(nameof(signatureValidator));
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute(byte[] signature, HttpRequest request)
        {
            await InnerExecute(signature, request);
            return new OkResult();
        }

        private async Task InnerExecute(byte[] signature, HttpRequest request)
        {
            try
            {
                if (signature == null)
                {
                    _Logger.LogError("Signature error: null.");
                    return;
                }

                if (signature.Length == 0)
                {
                    _Logger.LogError("Signature error: Zero length.");
                    return;
                }

                using var reader = new StreamReader(request.Body);
                var payload = await reader.ReadToEndAsync();
                var args = _JsonSerializer.Deserialize<ReleaseTeksArgs>(payload);

                _Logger.LogDebug($"Body: {args}.");

                var workflow = _DbContextProvider
                    .KeyReleaseWorkflowStates
                    .FirstOrDefault(x => x.BucketId == args.BucketId);

                if (workflow == null)
                {
                    _Logger.LogError("Matching workflow not found.");
                    return;
                }

                _Logger.LogDebug("Matching workflow found.");

                if (!_KeyValidator.Validate(args, workflow))
                {
                    _Logger.LogError("Keys not valid.");
                    return;
                }

                if (!_SignatureValidator.Valid(signature, workflow, Encoding.UTF8.GetBytes(payload)))
                {
                    _Logger.LogError($"Signature not valid: {Convert.ToBase64String(signature)}.");
                    return;
                }

                _Logger.LogDebug("Writing.");
                await _Writer.Execute(args);
                _Logger.LogDebug("Committing.");
                _DbContextProvider.SaveAndCommit();
                _Logger.LogDebug("Committed.");
            }
            catch (Exception e)
            {
                _Logger.LogError(e.ToString());
            }
        }
    }
}