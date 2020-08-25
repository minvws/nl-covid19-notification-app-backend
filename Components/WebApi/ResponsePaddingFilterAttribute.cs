// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    /// <summary>
    /// NOTE: DO not apply this attribute directly, apply <see cref="ResponsePaddingFilterFactoryAttribute"/>
    /// When applied to a controller or action which returns an OkObjectResult or OkResult
    /// adds a random padding to the response as a header.
    /// It's not in the list of headers in the RFC or new headers on the IANA site:
    /// https://www.iana.org/assignments/message-headers/message-headers.xhtml
    /// </summary>
    public class ResponsePaddingFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Custom header without the x-prefix as per https://tools.ietf.org/html/rfc6648.
        /// </summary>
        private const string PaddingHeader = "padding";

        private readonly IResponsePaddingConfig _Config;
        private readonly IRandomNumberGenerator _Rng;
        private readonly ILogger _Logger;
        private readonly IPaddingGenerator _PaddingGenerator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResponsePaddingFilterAttribute(IResponsePaddingConfig config, IRandomNumberGenerator rng, ILogger<ResponsePaddingFilterAttribute> logger, IPaddingGenerator paddingGenerator)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _PaddingGenerator  = paddingGenerator  ?? throw new ArgumentNullException(nameof(paddingGenerator));
        }

        /// <summary>
        /// Adds a random padding as an http header to the response.
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            
            string resultString = string.Empty;

            // Only works for object results
            if (context.Result is ObjectResult objectResult && objectResult.Value is string objectResultString)
            {
                resultString = objectResultString;
            }

            // Nothing needs doing if we're above the minimum length
            if (resultString.Length >= _Config.MinimumLengthInBytes)
            {
                _Logger.LogInformation("No padding needed as response length of {Length} is greater than the minimum of {MinimumLengthInBytes}..",
                    resultString.Length, _Config.MinimumLengthInBytes);

                return;
            }

            // Calculate length of padding to add
            var paddingLength = _Rng.Next(_Config.MinimumLengthInBytes, _Config.MaximumLengthInBytes) - resultString.Length;
            _Logger.LogInformation("Length of response padding:{PaddingLength}", paddingLength);

            // Get the padding bytes
            var padding = _PaddingGenerator.Generate(paddingLength);
            _Logger.LogDebug("Response padding:{Padding}", padding);

            // Add padding here
            context.HttpContext.Response.Headers.Add(PaddingHeader, padding);

            _Logger.LogInformation("Added padding to the response.");
        }
    }
}