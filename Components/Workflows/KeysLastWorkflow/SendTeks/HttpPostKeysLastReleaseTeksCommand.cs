// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class HttpPostKeysLastReleaseTeksCommand
    {
        private readonly IKeysLastReleaseTeksValidator _KeyValidator;
        private readonly IKeysLastSignatureValidator _SignatureValidator;
        private readonly IKeysLastTekWriter _Writer;
        private readonly WorkflowDbContext _DbContextProvider;

        public HttpPostKeysLastReleaseTeksCommand(IKeysLastReleaseTeksValidator keyValidator, IKeysLastSignatureValidator signatureValidator, IKeysLastTekWriter writer, WorkflowDbContext dbContextProvider)
        {
            _KeyValidator = keyValidator;
            _SignatureValidator = signatureValidator;
            _Writer = writer;
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute(byte[] signature, string payload)
        {
            var args = JsonConvert.DeserializeObject<KeysLastReleaseTeksArgs>(payload);

            if (!_KeyValidator.Validate(args) || !_SignatureValidator.Valid(signature, args.BucketId, Encoding.UTF8.GetBytes(payload)))
                return new OkResult();

            await _Writer.Execute(args);
            _DbContextProvider.SaveAndCommit();

            return new OkResult();
        }
    }
}