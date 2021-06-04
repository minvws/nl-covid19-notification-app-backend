// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
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

        private readonly IResponsePaddingConfig _config;
        private readonly IRandomNumberGenerator _rng;
        private readonly ResponsePaddingLoggingExtensions _logger;
        private readonly IPaddingGenerator _paddingGenerator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResponsePaddingFilterAttribute(IResponsePaddingConfig config, IRandomNumberGenerator rng, ResponsePaddingLoggingExtensions logger, IPaddingGenerator paddingGenerator)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paddingGenerator = paddingGenerator ?? throw new ArgumentNullException(nameof(paddingGenerator));
        }

        /// <summary>
        /// Adds a random padding as an http header to the response.
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            var resultString = string.Empty;

            // Only works for object results
            if (context.Result is ObjectResult objectResult && objectResult.Value is string objectResultString)
            {
                resultString = objectResultString;
            }

            // Nothing needs doing if we're above the minimum length
            if (resultString.Length >= _config.ByteCountMinimum)
            {
                _logger.WriteNoPaddingNeeded(resultString.Length, _config.ByteCountMinimum);
                return;
            }

            // Calculate length of padding to add
            var paddingLength = _rng.Next(_config.ByteCountMinimum, _config.ByteCountMaximum) - resultString.Length;
            _logger.WriteLengthOfResponsePadding(paddingLength);

            // Get the padding bytes
            var padding = _paddingGenerator.Generate(paddingLength);
            _logger.WritePaddingContent(padding);

            // Add padding here
            context.HttpContext.Response.Headers.Add(PaddingHeader, padding);
            _logger.WritePaddingAdded();
        }
    }
}