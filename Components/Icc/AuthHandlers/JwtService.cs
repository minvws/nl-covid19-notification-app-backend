// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class JwtService : IJwtService
    {
        private readonly IIccPortalConfig _IccPortalConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger<JwtService> _Logger;

        public JwtService(IIccPortalConfig iccPortalConfig, IUtcDateTimeProvider utcDateTimeProvider,
            ILogger<JwtService> logger)
        {
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
            _DateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private JwtBuilder CreateBuilder()
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_IccPortalConfig.JwtSecret)
                .MustVerifySignature();
        }

        public string Generate(ulong exp, Dictionary<string, object> claims)
        {
            //TODO nay validation on exp?
            if (claims == null) throw new ArgumentNullException(nameof(claims));
            // any further validation of claims?

            var builder = CreateBuilder();
            builder.AddClaim("exp", exp.ToString());

            foreach (var (key, value) in claims)
            {
                builder.AddClaim(key, value);
            }

            return builder.Encode();
        }

        public string Generate(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var builder = CreateBuilder();
            builder.AddClaim("exp", _DateTimeProvider.Now().AddHours(_IccPortalConfig.ClaimLifetimeHours).ToUnixTime());
            builder.AddClaim("id", GetClaimValue(claimsPrincipal, ClaimTypes.NameIdentifier));
            builder.AddClaim("email", GetClaimValue(claimsPrincipal, ClaimTypes.Email));
            builder.AddClaim("access_token",
                GetClaimValue(claimsPrincipal, "http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken"));
            builder.AddClaim("name",
                GetClaimValue(claimsPrincipal, "http://schemas.u2uconsult.com/ws/2014/04/identity/claims/displayname"));
            return builder.Encode();
        }

        private string? GetClaimValue(ClaimsPrincipal cp, string claimType) =>
            cp.Claims.FirstOrDefault(c => c.Type.Equals(claimType))?.Value;

        private static byte[][] GetBytes(IEnumerable<string> input) =>
            input.Select(b => Encoding.UTF8.GetBytes(b)).ToArray();

        public bool IsValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(nameof(token));

            if (token == null)
                return false;

            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var urlEncoder = new JwtBase64UrlEncoder();
            var decoder = new JwtDecoder(serializer, urlEncoder);
            var algFactory = new HMACSHAAlgorithmFactory();
            IJwtValidator validator = new JwtValidator(serializer, provider);

            var jwt = new JwtParts(token);

            var decodedPayload = Encoding.UTF8.GetString(urlEncoder.Decode(jwt.Payload));
            var decodedSignature = urlEncoder.Decode(jwt.Signature);

            var header = decoder.DecodeHeader<JwtHeader>(jwt);
            var alg = algFactory.Create(JwtDecoderContext.Create(header, decodedPayload, jwt)) ??
                      throw new ArgumentNullException(
                          "algFactory.Create(JwtDecoderContext.Create(header, decodedPayload, jwt))");
            
            var bytesToSign = Encoding.UTF8.GetBytes((String.Concat(jwt.Header, ".", jwt.Payload)));
            bool result;
            var secret = new[] {_IccPortalConfig.JwtSecret};
            if (alg is IAsymmetricAlgorithm asymmAlg)
            {
                result = validator.TryValidate(decodedPayload, asymmAlg, bytesToSign, decodedSignature, out var ex);
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

                result = validator.TryValidate(decodedPayload, rawSignature, recreatedSignatures, out var ex);
                if (ex != null)
                    _Logger.LogWarning(ex.Message + " – token validation");
            }

            return result;
        }

        public IDictionary<string, string> Decode(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(nameof(token));
            return CreateBuilder().Decode<IDictionary<string, object>>(token)
                .ToDictionary(x => x.Key, x => x.Value.ToString());
        }
    }
}