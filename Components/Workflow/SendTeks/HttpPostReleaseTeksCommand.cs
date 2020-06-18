// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public HttpPostReleaseTeksCommand(IReleaseTeksValidator keyValidator, ISignatureValidator signatureValidator, ITekWriter writer, WorkflowDbContext dbContextProvider)
        {
            _KeyValidator = keyValidator;
            _SignatureValidator = signatureValidator;
            _Writer = writer;
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute(byte[] signature, string payload)
        {
            var args = JsonConvert.DeserializeObject<ReleaseTeksArgs>(payload);

            if (!_KeyValidator.Validate(args) || !_SignatureValidator.Valid(signature, args.BucketId, Encoding.UTF8.GetBytes(payload)))
                return new OkResult();

            await _Writer.Execute(args);
            _DbContextProvider.SaveAndCommit();

            return new OkResult();
        }
    }
}