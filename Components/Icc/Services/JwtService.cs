// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services
{
    public class JwtService
    {
        private readonly JwtBuilder _JwtBuilder;

        public JwtService(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var secret = configuration.GetSection("IccPortalConfig:Jwt:secret").Value; //TODO Not a section!
            _JwtBuilder = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .MustVerifySignature();
        }

        public string GenerateCustomJwt(long exp, Dictionary<string, object>? claims = null)
        {
            _JwtBuilder.AddClaim("exp", exp.ToString());
            if (claims != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in claims)
                {
                    _JwtBuilder.AddClaim(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return _JwtBuilder.Encode();
        }

        public string GenerateJwt(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _JwtBuilder.AddClaim("exp", DateTimeOffset.UtcNow.AddHours(3).ToUnixTimeSeconds()); // default
            // add identityhub claims
            var claims = user.Claims.ToList();
            // _builder.AddClaims();
            _JwtBuilder.AddClaim("access_token", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken"))
                ?.Value);
            _JwtBuilder.AddClaim("id", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"))
                ?.Value);
            _JwtBuilder.AddClaim("name", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.u2uconsult.com/ws/2014/04/identity/claims/displayname"))
                ?.Value);
            _JwtBuilder.AddClaim("email", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))
                ?.Value);


            return _JwtBuilder.Encode();
        }

        public bool IsValidJwt(string token) //TODO nulls allowed?
        {
            var payload = DecodeJwt(token);
            return payload.Keys.Contains("access_token") && payload["access_token"].ToString().Length > 0;
        }

        public IDictionary<string, object> DecodeJwt(string token) //TODO nulls allowed?
        {
            return _JwtBuilder.Decode<IDictionary<string, object>>(token);
        }
    }
}