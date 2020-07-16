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
        private IConfiguration _Configuration;


        private string _secret;
        private JwtBuilder _builder;


        public JwtService(IConfiguration configuration)
        {
            _Configuration = configuration;

            _secret = _Configuration.GetSection("IccPortalConfig:Jwt:secret").Value;
            _builder = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_secret)
                .MustVerifySignature();
        }

        public string GenerateCustomJwt(long exp, Dictionary<string, object>? claims = null)
        {
            _builder.AddClaim("exp", exp.ToString());
            if (claims != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in claims)
                {
                    _builder.AddClaim(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return _builder.Encode();
        }

        public string GenerateJwt(ClaimsPrincipal user)
        {
            _builder.AddClaim("exp", DateTimeOffset.UtcNow.AddHours(3).ToUnixTimeSeconds()); // default
            // add identityhub claims
            var claims = user.Claims.ToList();
            // _builder.AddClaims();
            _builder.AddClaim("access_token", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken"))
                ?.Value);
            _builder.AddClaim("id", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"))
                ?.Value);
            _builder.AddClaim("name", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.u2uconsult.com/ws/2014/04/identity/claims/displayname"))
                ?.Value);
            _builder.AddClaim("email", claims.FirstOrDefault(c =>
                    c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))
                ?.Value);


            return _builder.Encode();
        }

        public bool IsValidJwt(string token, string checkElement= "access_token")
        {
            var payload = DecodeJwt(token);

            return payload.Keys.Contains(checkElement) && payload[checkElement].ToString().Length > 0;
        }

        public IDictionary<string, object> DecodeJwt(string token)
        {
            return _builder.Decode<IDictionary<string, object>>(token);
        }
    }
}