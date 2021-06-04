// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp
{
    public class PolicyAuthorizationOptions
    {
        private const string TelefonistRoleProd = "C19NA-Telefonist";
        private const string TelefonistRoleTest = "C19NA-Telefonist-Test";
        private const string BeheerRoleProd = "C19NA-Beheer";
        private const string BeheerRoleTest = "C19NA-Beheer-Test";

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIccPortalConfig _iccPortalConfig;
        private readonly List<string> _allowedBeheerRoleValues = new List<string>();
        private readonly List<string> _allowedTelefonistRoleValues = new List<string>();

        public PolicyAuthorizationOptions(IWebHostEnvironment webHostEnvironment, IIccPortalConfig iccPortalConfig)
        {
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _iccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
        }

        private void PolicyGenerator()
        {
            if (_webHostEnvironment.IsProduction())
            {
                _allowedTelefonistRoleValues.Add(TelefonistRoleProd);
                _allowedBeheerRoleValues.Add(BeheerRoleProd);
                return;
            }

            if (_webHostEnvironment.IsStaging() || _webHostEnvironment.IsDevelopment())
            {
                _allowedTelefonistRoleValues.Add(TelefonistRoleTest);
                _allowedBeheerRoleValues.Add(BeheerRoleTest);
                return;
            }
            throw new InvalidOperationException("Environment not recognized.");
        }

        public void Build(AuthorizationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (_iccPortalConfig.StrictRolePolicyEnabled)
            {
                PolicyGenerator();
            }

            options.AddPolicy("TelefonistRole",
                builder => builder.RequireClaim(ClaimTypes.Role, _allowedTelefonistRoleValues));
            options.AddPolicy("BeheerRole",
                builder => builder.RequireClaim(ClaimTypes.Role, _allowedBeheerRoleValues));
        }
    }

}