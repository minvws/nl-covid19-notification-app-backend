// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class HttpPostGenerateExposureKeySetsCommand
    {
        private readonly WorkflowDbContext _Input;
        private readonly ExposureContentDbContext _Output;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IGaenContentConfig _GaenContentConfig;
        private readonly IExposureKeySetHeaderInfoConfig _HsmExposureKeySetHeaderInfoConfig;
        private readonly IContentSigner _EcdSaSigner;
        private readonly IContentSigner _CmsSigner;

        public HttpPostGenerateExposureKeySetsCommand(
            WorkflowDbContext input, 
            ExposureContentDbContext output,
            IUtcDateTimeProvider utcDateTimeProvider,
            IGaenContentConfig gaenContentConfig,
            IExposureKeySetHeaderInfoConfig hsmExposureKeySetHeaderInfoConfig,
            IContentSigner ecdSaSigner, //ecdsa
            IContentSigner cmsSigner) //cms
        {
            _Input = input;
            _Output = output;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _GaenContentConfig = gaenContentConfig;
            _HsmExposureKeySetHeaderInfoConfig = hsmExposureKeySetHeaderInfoConfig;
            _EcdSaSigner = ecdSaSigner;
            _CmsSigner = cmsSigner;
        }

        public async Task<IActionResult> Execute(bool useAllKeys = false)
        {
            using var bb = new ExposureKeySetBatchJobMk2(
                _GaenContentConfig,
                new ExposureKeySetBuilderV1(
                    _HsmExposureKeySetHeaderInfoConfig,
                    _EcdSaSigner, _CmsSigner, _UtcDateTimeProvider, new GeneratedProtobufContentFormatter()),
                _Input,
                _Output,
                _UtcDateTimeProvider,
                new StandardPublishingIdFormatter()
            );

            try
            {
                await bb.Execute(useAllKeys);
            }
            catch (Exception e)
            {
                throw;
            }

            return new OkResult();
        }
    }
}
