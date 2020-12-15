// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    // NOTE: do not apply this attribute directly, apply DecoyTimeGeneratorAttributeFactory
    public class DecoyTimeGeneratorAttribute : ActionFilterAttribute
    {
        private readonly DecoyKeysLoggingExtensions _Logger;
        private readonly IRandomNumberGenerator _RandomNumberGenerator;
        private readonly IDecoyKeysConfig _Config;

        public DecoyTimeGeneratorAttribute(
            DecoyKeysLoggingExtensions logger,
            IRandomNumberGenerator randomNumberGenerator,
            IDecoyKeysConfig config)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _RandomNumberGenerator = randomNumberGenerator ?? throw new ArgumentNullException(nameof(randomNumberGenerator));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var delayMs = _RandomNumberGenerator.Next(_Config.MinimumDelayInMilliseconds, _Config.MaximumDelayInMilliseconds);
            
            _Logger.WriteDelaying(delayMs);
            await Task.Delay(delayMs);
            await next();
        }
    }
}