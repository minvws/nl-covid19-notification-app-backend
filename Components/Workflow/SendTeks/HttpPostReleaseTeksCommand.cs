// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class HttpPostReleaseTeksCommand
    {
        private readonly IReleaseTeksValidator _KeyValidator;
        private readonly ISignatureValidator _SignatureValidator;
        private readonly ITekWriter _Writer;
        private readonly WorkflowDbContext _DbContextProvider;

        public HttpPostReleaseTeksCommand(
            IReleaseTeksValidator keyValidator, 
            ISignatureValidator signatureValidator, 
            ITekWriter writer, 
            WorkflowDbContext dbContextProvider)
        {
            _KeyValidator = keyValidator;
            _SignatureValidator = signatureValidator;
            _Writer = writer;
            _DbContextProvider = dbContextProvider;
        }

        public async Task Execute(byte[] signature, HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var payload = await reader.ReadToEndAsync();

            var args = JsonConvert.DeserializeObject<ReleaseTeksArgs>(payload);

            var workflow = _DbContextProvider
                .KeyReleaseWorkflowStates
                .FirstOrDefault(x => x.BucketId == args.BucketId);

            if (workflow == null || !_KeyValidator.Validate(args, workflow) ||
                !_SignatureValidator.Valid(signature, workflow, Encoding.UTF8.GetBytes(payload)))
                return;

            await _Writer.Execute(args);

            _DbContextProvider.SaveAndCommit();
        }
    }
}