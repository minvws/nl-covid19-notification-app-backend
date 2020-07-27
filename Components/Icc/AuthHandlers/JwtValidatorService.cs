// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    class JwtValidatorService : IJwtValidatorService
    {
        private readonly ILogger<IJwtValidatorService> _Logger;
        private readonly IIccPortalConfig _IccPortalConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        private readonly IJsonSerializer _Serializer;
        private readonly UtcDateTimeProvider _Provider;
        private readonly JwtBase64UrlEncoder _UrlEncoder;
        private readonly JwtDecoder _Decoder;
        private readonly HMACSHAAlgorithmFactory _AlgFactory;
        private readonly JwtValidator _Validator;


        public JwtValidatorService(IIccPortalConfig iccPortalConfig, IUtcDateTimeProvider utcDateTimeProvider,
            ILogger<IJwtValidatorService> logger)
        {
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
            _DateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _Serializer = new JsonNetSerializer();
            _Provider = new UtcDateTimeProvider();
            _UrlEncoder = new JwtBase64UrlEncoder();
            _Decoder = new JwtDecoder(_Serializer, _UrlEncoder);
            _AlgFactory = new HMACSHAAlgorithmFactory();
            _Validator = new JwtValidator(_Serializer, _Provider);
        }
        public bool IsValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(nameof(token));

            if (token == null)
                return false;
            
            var jwt = new JwtParts(token);

            var decodedPayload = Encoding.UTF8.GetString(_UrlEncoder.Decode(jwt.Payload));
            var decodedSignature = _UrlEncoder.Decode(jwt.Signature);

            var header = _Decoder.DecodeHeader<JwtHeader>(jwt);
            var alg = _AlgFactory.Create(JwtDecoderContext.Create(header, decodedPayload, jwt)) ??
                      throw new ArgumentNullException(
                          "algFactory.Create(JwtDecoderContext.Create(header, decodedPayload, jwt))");

            var bytesToSign = Encoding.UTF8.GetBytes((String.Concat(jwt.Header, ".", jwt.Payload)));
            bool result;
            var secret = new[] {_IccPortalConfig.JwtSecret};
            if (alg is IAsymmetricAlgorithm asymmAlg)
            {
                result = _Validator.TryValidate(decodedPayload, asymmAlg, bytesToSign, decodedSignature, out var ex);
                if (ex != null)
                    _Logger.LogWarning(ex.Message + " – token validation");
            }
            else
            {
                // the signature on the token, with the leading =
                var rawSignature = Convert.ToBase64String(decodedSignature);

                // the signatures re-created by the algorithm, with the leading =

                var keys = secret.Select(s => Encoding.UTF8.GetBytes(s)).ToArray();

                var recreatedSignatures = keys.Select(key => alg.Sign(key, bytesToSign))
                    .Select(sd => Convert.ToBase64String(sd))
                    .ToArray();

                result = _Validator.TryValidate(decodedPayload, rawSignature, recreatedSignatures, out var ex);
                if (ex != null)
                    _Logger.LogWarning(ex.Message + " – token validation");
            }

            return result;
        }
    }
}