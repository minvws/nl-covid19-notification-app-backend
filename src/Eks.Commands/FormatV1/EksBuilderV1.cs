// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1
{
    /// <summary>
    /// Building EKS GAEN content
    /// </summary>
    public class EksBuilderV1 : IEksBuilder
    {
        private const string Header = "EK Export v1    ";

        private const string GaenSignatureOid = "1.2.840.10045.4.3.2";

        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IEksContentFormatter _eksContentFormatter;
        private readonly IEksHeaderInfoConfig _config;
        private readonly IHsmSignerService _hsmSignerService;
        private readonly ILogger _logger;

        public EksBuilderV1(
            IEksHeaderInfoConfig headerInfoConfig,
            IUtcDateTimeProvider dateTimeProvider,
            IEksContentFormatter eksContentFormatter,
            IHsmSignerService hsmSignerService,
            ILogger<EksBuilderV1> logger
            )
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _eksContentFormatter = eksContentFormatter ?? throw new ArgumentNullException(nameof(eksContentFormatter));
            _config = headerInfoConfig ?? throw new ArgumentNullException(nameof(headerInfoConfig));
            _hsmSignerService = hsmSignerService ?? throw new ArgumentNullException(nameof(hsmSignerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Builds content with Gaen signed data and signing files
        /// </summary>
        /// <param name="keys">All TEks</param>
        /// <returns>Gaen data</returns>
        public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Any(x => x == null))
            {
                throw new ArgumentException("At least one key is null.", nameof(keys));
            }

            var securityInfo = GetGaenSignatureInfo(GaenSignatureOid, _config.VerificationKeyVersion);
            var content = CreateContent(keys, securityInfo);
            var zippedContent = await CreateZippedContent(content, securityInfo);

            return zippedContent;
        }

        private ExposureKeySetContentArgs CreateContent(TemporaryExposureKeyArgs[] keys, SignatureInfoArgs securityInfo)
        {
            var content = new ExposureKeySetContentArgs
            {
                Header = Header,
                Region = "NL",
                BatchNum = 1,
                BatchSize = 1,
                SignatureInfos = new[] { securityInfo },
                StartTimestamp = _dateTimeProvider.Snapshot.AddDays(-1).ToUnixTimeU64(),
                EndTimestamp = _dateTimeProvider.Snapshot.ToUnixTimeU64(),
                Keys = keys
            };

            return content;
        }

        private async Task<byte[]> CreateZippedContent(ExposureKeySetContentArgs exposureKeySetContentArgs, SignatureInfoArgs securityInfoArgs)
        {
            var contentBytes = _eksContentFormatter.GetBytes(exposureKeySetContentArgs);

            var cmsSignature = await _hsmSignerService.GetNlCmsSignatureAsync(contentBytes);
            var gaenSignature = await _hsmSignerService.GetGaenSignatureAsync(contentBytes);

            _logger.LogDebug("CMS Sig: {CmsSignature}", Convert.ToBase64String(cmsSignature));
            _logger.LogDebug("GAEN Sig: {GaenSignature}", Convert.ToBase64String(gaenSignature));

            var signatures = new ExposureKeySetSignaturesContentArgs
            {
                Items = new[]
                {
                    new ExposureKeySetSignatureContentArgs
                    {
                        SignatureInfo = securityInfoArgs,
                        Signature = gaenSignature,
                        BatchSize = exposureKeySetContentArgs.BatchSize,
                        BatchNum = exposureKeySetContentArgs.BatchNum
                    }
                }
            };

            var gaenSigFile = _eksContentFormatter.GetBytes(signatures);
            var zippedContent = await new ZippedContentBuilder().BuildEksAsync(contentBytes, gaenSigFile, cmsSignature);
            return zippedContent;
        }

        private SignatureInfoArgs GetGaenSignatureInfo(string signatureOid, string verificationKeyVersion)
            => new()
            {
                SignatureAlgorithm = signatureOid,
                VerificationKeyId = _config.VerificationKeyId,
                VerificationKeyVersion = verificationKeyVersion,
                AppBundleId = _config.AppBundleId
            };
    }
}
