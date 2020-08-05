using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    /// <summary>
    /// When applied to a controller or action which returns an OkObjectResult or OkResult
    /// adds a random padding to the response as a header.
    /// It's not in the list of headers in the RFC or new headers on the IANA site:
    /// https://www.iana.org/assignments/message-headers/message-headers.xhtml
    /// </summary>
    public class ResponsePaddingFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Custom header without the x-prefix as per https://tools.ietf.org/html/rfc6648.
        /// </summary>
        private const string PaddingHeader = "padding";

        /// <summary>
        /// Character used for padding, must be a 1-byte character
        /// </summary>
        private const string PaddingCharacter = "=";
        
        private readonly IResponsePaddingConfig _Config;
        private readonly IRandomNumberGenerator _Rng;
        private readonly ILogger _Logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResponsePaddingFilter(IResponsePaddingConfig config, IRandomNumberGenerator rng, ILogger<ResponsePaddingFilter> logger)
        {
            _Config = config;
            _Rng = rng;
            _Logger = logger;
        }

        /// <summary>
        /// Adds a random padding as an http header to the response.
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            if (_Config == null) throw new ArgumentNullException(nameof(_Config));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            string resultString = string.Empty;

            // Only works for object results
            if (context.Result is ObjectResult objectResult &&  objectResult.Value is string objectResultString)
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
            
            // Add padding here
            context.HttpContext.Response.Headers.Add(PaddingHeader, Padding(resultString.Length));

            _Logger.LogInformation("Added padding to the response.");
        }

        /// <summary>
        /// Adds padding equal 
        /// </summary>
        private string Padding(int contentLength)
        {
            if (_Config == null) throw new ArgumentNullException(nameof(_Config));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));
            if (_Rng == null) throw new ArgumentNullException(nameof(_Rng));

            var paddingLength = _Rng.Next(_Config.MinimumLengthInBytes, _Config.MaximumLengthInBytes) - contentLength;
            _Logger.LogInformation("Length of response padding: {PaddingLength}", paddingLength);

            var padding = string.Concat(Enumerable.Repeat(PaddingCharacter, paddingLength));
            _Logger.LogDebug("Response padding: {Padding}", padding);

            return padding;
        }
    }
}