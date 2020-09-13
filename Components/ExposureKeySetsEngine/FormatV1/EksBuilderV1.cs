// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1
{
    public class EksBuilderV1 : IEksBuilder
    {
        private const string Header = "EK Export v1    ";

        private readonly IGaContentSigner _GaenContentSigner;
        private readonly IContentSigner _NlContentSigner;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IEksContentFormatter _EksContentFormatter;
        private readonly IEksHeaderInfoConfig _Config;
        private readonly ILogger _Logger;

        public EksBuilderV1(
            IEksHeaderInfoConfig headerInfoConfig,
            IGaContentSigner gaenContentSigner,
            IContentSigner nlContentSigner,
            IUtcDateTimeProvider dateTimeProvider,
            IEksContentFormatter eksContentFormatter,
            ILogger<EksBuilderV1> logger
            )
        {
            _GaenContentSigner = gaenContentSigner ?? throw new ArgumentNullException(nameof(gaenContentSigner));
            _NlContentSigner = nlContentSigner ?? throw new ArgumentNullException(nameof(nlContentSigner));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _EksContentFormatter = eksContentFormatter ?? throw new ArgumentNullException(nameof(eksContentFormatter));
            _Config = headerInfoConfig ?? throw new ArgumentNullException(nameof(headerInfoConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (keys.Any(x => x == null)) throw new ArgumentException("At least one key in null.", nameof(keys));

            var securityInfo = GetGaenSignatureInfo();

            var content = new ExposureKeySetContentArgs
            {
                Header = Header,
                Region = "NL",
                BatchNum = 1,
                BatchSize = 1,
                SignatureInfos = new[] {securityInfo},
                StartTimestamp = _DateTimeProvider.Snapshot.AddDays(-1).ToUnixTimeU64(),
                EndTimestamp = _DateTimeProvider.Snapshot.ToUnixTimeU64(),
                Keys = keys
            };

            var contentBytes = _EksContentFormatter.GetBytes(content);
            var nlSig = _NlContentSigner.GetSignature(contentBytes);
            var gaenSig = _GaenContentSigner.GetSignature(contentBytes);

            _Logger.LogDebug("GAEN Sig: {Convert.ToBase64String(GaenSig)}.", gaenSig);
            _Logger.LogDebug("NL Sig: {Convert.ToBase64String(NlSig)}.", nlSig);

            var signatures = new ExposureKeySetSignaturesContentArgs
            {
                Items = new[]
                {
                    new ExposureKeySetSignatureContentArgs
                    {
                        SignatureInfo = securityInfo,
                        Signature = gaenSig,
                        BatchSize = content.BatchSize,
                        BatchNum = content.BatchNum
                    },
                }
            };

            var gaenSigFile = _EksContentFormatter.GetBytes(signatures);
            return await new ZippedContentBuilder().BuildEks(contentBytes, gaenSigFile, nlSig);
        }

        private SignatureInfoArgs GetGaenSignatureInfo()
            => new SignatureInfoArgs
            {
                SignatureAlgorithm = _GaenContentSigner.SignatureOid,
                VerificationKeyId = _Config.VerificationKeyId,
                VerificationKeyVersion = _Config.VerificationKeyVersion,
                AppBundleId = _Config.AppBundleId
            };
    }
}
