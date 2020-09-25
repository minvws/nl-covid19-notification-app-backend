// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;

    // NOTE: DO not apply this attribute directly, apply DecoyTimeGeneratorAttributeFactory
    public class DecoyTimeGeneratorAttribute : ActionFilterAttribute
    {
        private readonly ILogger _Logger;
        private readonly IRandomNumberGenerator _RandomNumberGenerator;
        private readonly IDecoyKeysConfig _Config;

        public DecoyTimeGeneratorAttribute(
            ILogger<DecoyTimeGeneratorAttribute> logger,
            IRandomNumberGenerator randomNumberGenerator,
            IDecoyKeysConfig config)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _RandomNumberGenerator = randomNumberGenerator ?? throw new ArgumentNullException(nameof(randomNumberGenerator));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        //Todo: check whether to execute before result is given.
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            base.OnActionExecuting(context);
            var result = next();

            var delayInMilliseconds = _RandomNumberGenerator.Next(_Config.MinimumDelayInMilliseconds, _Config.MaximumDelayInMilliseconds);
            _Logger.LogDebug("Delaying for {DelayInMilliseconds} seconds", delayInMilliseconds);

            return new Task(() => Task.Delay(delayInMilliseconds));
        }
    }
}